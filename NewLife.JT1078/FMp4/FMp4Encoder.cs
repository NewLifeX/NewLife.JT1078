using NewLife.Video.Codecs;
using NewLife.Video.FMp4;
using NewLife.Video.FMp4.Enums;
using NewLife.Video.FMp4.MessagePack;
using NewLife.Video.FMp4.Samples;
using NewLife.JT1078.H264;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.FMp4;

/// <summary>
/// FMp4编码
/// fmp4
/// stream data 
/// ref: https://www.w3.org/TR/mse-byte-stream-format-isobmff/#movie-fragment-relative-addressing
/// </summary>
public class FMp4Encoder
{
    private readonly Dictionary<String, TrackInfo> TrackInfos;
    private const UInt32 TfhdFlags = Fmp4Constants.TFHD_FLAG_DEFAULT_BASE_IS_MOOF |
                           Fmp4Constants.TFHD_FLAG_DEFAULT_SIZE |
                           Fmp4Constants.TFHD_FLAG_SAMPLE_DESCRIPTION_INDEX |
                           Fmp4Constants.TFHD_FLAG_DEFAULT_DURATION |
                           Fmp4Constants.TFHD_FLAG_DEFAULT_FLAGS;
    private const UInt32 TrunFlags = Fmp4Constants.TRUN_FLAG_DATA_OFFSET_PRESENT |
                           Fmp4Constants.TRUN_FLAG_SAMPLE_SIZE_PRESENT |
                           Fmp4Constants.TRUN_FLAG_FIRST_SAMPLE_FLAGS_PRESENT;
    private const UInt32 SampleDescriptionIndex = 1;
    private const UInt32 DefaultSampleDuration = 40;
    private const UInt32 VideoTrackID = 1;
    private const Int32 CacheSize = 1024 * 1024;
    private readonly H264Decoder h264Decoder = new H264Decoder();

    /// <summary></summary>
    public FMp4Encoder() => TrackInfos = new Dictionary<String, TrackInfo>(StringComparer.OrdinalIgnoreCase);

    /// <summary>编码ftyp盒子</summary>
    /// <returns></returns>
    internal Byte[] FtypBox()
    {
        var buffer = Fmp4ArrayPool.Rent(1024);
        var writer = new Fmp4MessagePackWriter(buffer);
        try
        {
            //ftyp
            var fileTypeBox = new FileTypeBox
            {
                MajorBrand = "msdh",
                MinorVersion = "\0\0\0\0"
            };
            fileTypeBox.CompatibleBrands.Add("isom");
            fileTypeBox.CompatibleBrands.Add("mp42");
            fileTypeBox.CompatibleBrands.Add("msdh");
            fileTypeBox.CompatibleBrands.Add("msix");
            // default‐base is‐moof flag
            fileTypeBox.CompatibleBrands.Add("iso5");
            // styp
            fileTypeBox.CompatibleBrands.Add("iso6");
            fileTypeBox.ToBuffer(ref writer);

            return writer.FlushAndGetArray();
        }
        finally
        {
            Fmp4ArrayPool.Return(buffer);
        }
    }

    /// <summary>编码moov盒子</summary>
    /// <returns></returns>
    internal Byte[] MoovBox(in H264NALU sps, in H264NALU pps)
    {
        var buffer = Fmp4ArrayPool.Rent(sps.RawData.Length + pps.RawData.Length + 1024);
        var writer = new Fmp4MessagePackWriter(buffer);
        try
        {
            var rawData = h264Decoder.DiscardEmulationPreventionBytes(sps.RawData);
            if (!H264SpsParser.TryParse(rawData, out var spsInfo)) return null;
            //moov
            var movieBox = new MovieBox
            {
                MovieHeaderBox = new MovieHeaderBox(0, 2)
                {
                    CreationTime = 0,
                    ModificationTime = 0,
                    Duration = 0,
                    Timescale = 1000,
                    NextTrackID = 99
                },
                TrackBox = new TrackBox
                {
                    TrackHeaderBox = new TrackHeaderBox(0, 3)
                    {
                        CreationTime = 0,
                        ModificationTime = 0,
                        TrackID = VideoTrackID,
                        Duration = 0,
                        TrackIsAudio = false,
                        Width = (UInt32)spsInfo.width,
                        Height = (UInt32)spsInfo.height
                    },
                    MediaBox = new MediaBox
                    {
                        MediaHeaderBox = new MediaHeaderBox
                        {
                            CreationTime = 0,
                            ModificationTime = 0,
                            Timescale = 1000,
                            Duration = 0
                        },
                        HandlerBox = new HandlerBox
                        {
                            HandlerType = HandlerType.vide,
                            Name = "VideoHandler"
                        },
                        MediaInformationBox = new MediaInformationBox
                        {
                            VideoMediaHeaderBox = new VideoMediaHeaderBox(),
                            DataInformationBox = new DataInformationBox
                            {
                                DataReferenceBox = new DataReferenceBox
                                {
                                    DataEntryBoxes = new List<DataEntryBox>
                                    {
                                        new DataEntryUrlBox(1)
                                    }
                                }
                            },
                            SampleTableBox = new SampleTableBox
                            {
                                SampleDescriptionBox = new SampleDescriptionBox
                                {
                                    SampleEntries = new List<SampleEntry>()
                                }
                            }
                        }
                    }
                }
            };
            var avc1 = new AVC1SampleEntry
            {
                AVCConfigurationBox = new AVCConfigurationBox(),
                //h264
                Width = (UInt16)movieBox.TrackBox.TrackHeaderBox.Width,
                Height = (UInt16)movieBox.TrackBox.TrackHeaderBox.Height
            };
            avc1.AVCConfigurationBox.AVCLevelIndication = spsInfo.levelIdc;
            avc1.AVCConfigurationBox.AVCProfileIndication = spsInfo.profileIdc;
            avc1.AVCConfigurationBox.ProfileCompatibility = (Byte)spsInfo.profileCompat;
            avc1.AVCConfigurationBox.PPSs = new List<Byte[]>() { pps.RawData };
            avc1.AVCConfigurationBox.SPSs = new List<Byte[]>() { sps.RawData };
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SampleDescriptionBox.SampleEntries.Add(avc1);
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.TimeToSampleBox = new TimeToSampleBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SyncSampleBox = new SyncSampleBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SampleToChunkBox = new SampleToChunkBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SampleSizeBox = new SampleSizeBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.ChunkOffsetBox = new ChunkOffsetBox();
            movieBox.MovieExtendsBox = new MovieExtendsBox
            {
                TrackExtendsBoxs = new List<TrackExtendsBox>()
            };
            var trex = new TrackExtendsBox
            {
                TrackID = VideoTrackID,
                DefaultSampleDescriptionIndex = SampleDescriptionIndex,
                DefaultSampleDuration = 0,
                DefaultSampleSize = 0,
                DefaultSampleFlags = 0
            };
            movieBox.MovieExtendsBox.TrackExtendsBoxs.Add(trex);
            movieBox.ToBuffer(ref writer);

            return writer.FlushAndGetArray();
        }
        finally
        {
            Fmp4ArrayPool.Return(buffer);
        }
    }

    /// <summary>styp</summary>
    /// <returns></returns>
    internal Byte[] StypBox()
    {
        var buffer = Fmp4ArrayPool.Rent(1024);
        var writer = new Fmp4MessagePackWriter(buffer);
        try
        {
            var stypTypeBox = new SegmentTypeBox
            {
                MajorBrand = "msdh",
                MinorVersion = "\0\0\0\0"
            };
            stypTypeBox.CompatibleBrands.Add("isom");
            stypTypeBox.CompatibleBrands.Add("mp42");
            stypTypeBox.CompatibleBrands.Add("msdh");
            stypTypeBox.CompatibleBrands.Add("msix");
            stypTypeBox.CompatibleBrands.Add("iso5");
            stypTypeBox.CompatibleBrands.Add("iso6");
            stypTypeBox.ToBuffer(ref writer);

            return writer.FlushAndGetArray();
        }
        finally
        {
            Fmp4ArrayPool.Return(buffer);
        }
    }

    /// <summary>
    /// 编码其他视频数据盒子
    /// 注意：固定I帧解析,延迟一个I帧的时间
    /// I P P P P
    /// I P P P P 
    /// I P P P P
    /// </summary>
    /// <returns></returns>
    internal Byte[] OtherVideoBox(in List<JTMessage> nalus)
    {
        var buffer = Fmp4ArrayPool.Rent(nalus.Sum(s => s.Payload.Total) + 4096);
        var writer = new Fmp4MessagePackWriter(buffer);
        try
        {
            var truns = new List<TrackRunBox.TrackRunInfo>();
            var key = String.Empty;
            UInt64 timestamp = 0;
            UInt32 subsegmentDuration = 0;
            for (var i = 0; i < nalus.Count; i++)
            {
                var nalu = nalus[i];
                if (String.IsNullOrEmpty(key))
                {
                    key = nalu.GetKey();
                }
                UInt32 duration = 0;
                if (timestamp > 0)
                {
                    duration = (UInt32)(nalu.Timestamp - timestamp);
                }
                truns.Add(new TrackRunBox.TrackRunInfo()
                {
                    SampleDuration = duration,
                    SampleSize = (UInt32)(nalu.Payload.Total),
                });
                subsegmentDuration += duration;
                timestamp = nalu.Timestamp;
            }
            if (!TrackInfos.TryGetValue(key, out var trackInfo))
            {
                trackInfo = new TrackInfo { SN = 1, SubsegmentDuration = 0 };
                TrackInfos.Add(key, trackInfo);
            }
            if (trackInfo.SN == UInt32.MaxValue)
            {
                trackInfo.SN = 1;
            }
            trackInfo.SN++;
            var segmentIndexBox = new SegmentIndexBox(1)
            {
                ReferenceID = 1,
                EarliestPresentationTime = trackInfo.SubsegmentDuration,
                SegmentIndexs = new List<SegmentIndexBox.SegmentIndex>()
                {
                     new SegmentIndexBox.SegmentIndex
                     {
                          SubsegmentDuration=subsegmentDuration
                     }
                }
            };
            segmentIndexBox.ToBuffer(ref writer);

            var current1 = writer.GetCurrentPosition();
            //moof
            var movieFragmentBox = new MovieFragmentBox
            {
                MovieFragmentHeaderBox = new MovieFragmentHeaderBox
                {
                    SequenceNumber = trackInfo.SN
                },
                TrackFragmentBox = new TrackFragmentBox
                {
                    TrackFragmentHeaderBox = new TrackFragmentHeaderBox(TfhdFlags)
                    {
                        TrackID = VideoTrackID,
                        SampleDescriptionIndex = SampleDescriptionIndex,
                        DefaultSampleSize = truns[0].SampleSize,
                        DefaultSampleFlags = Fmp4Constants.TFHD_FLAG_VIDEO_TPYE
                    },
                    TrackFragmentBaseMediaDecodeTimeBox = new TrackFragmentBaseMediaDecodeTimeBox
                    {
                        BaseMediaDecodeTime = trackInfo.SubsegmentDuration
                    }
                }
            };
            trackInfo.SubsegmentDuration += subsegmentDuration;
            TrackInfos[key] = trackInfo;
            // trun
            movieFragmentBox.TrackFragmentBox.TrackRunBox = new TrackRunBox(1, flags: TrunFlags)
            {
                FirstSampleFlags = Fmp4Constants.TREX_FLAG_SAMPLE_DEPENDS_ON_I_PICTURE,
                TrackRunInfos = truns
            };
            movieFragmentBox.ToBuffer(ref writer);
            // mdat
            var mediaDataBox = new MediaDataBox
            {
                Data = new List<Byte[]>()
            };
            foreach (var nalu in nalus)
            {
                var h264NALUs = h264Decoder.ParseNALU(nalu);
                if (h264NALUs == null) continue;

                foreach (var n in h264NALUs)
                {
                    mediaDataBox.Data.Add(n.RawData);
                }
            }
            mediaDataBox.ToBuffer(ref writer);

            var current2 = writer.GetCurrentPosition();
            foreach (var postion in segmentIndexBox.ReferencedSizePositions)
            {
                writer.WriteUInt32Return((UInt32)(current2 - current1), postion);
            }

            return writer.FlushAndGetArray();
        }
        finally
        {
            Fmp4ArrayPool.Return(buffer);
        }
    }

    /// <summary>
    /// 编码首帧
    /// ftyp moov
    /// </summary>
    /// <param name="avframe"></param>
    /// <returns></returns>
    public Byte[] FirstVideoBox(in AVFrame avframe)
    {
        var buffer = Fmp4ArrayPool.Rent(CacheSize);
        var writer = new Fmp4MessagePackWriter(buffer);
        try
        {
            //ftyp
            var fileTypeBox = new FileTypeBox
            {
                MajorBrand = "msdh",
                MinorVersion = "\0\0\0\0"
            };
            fileTypeBox.CompatibleBrands.Add("isom");
            fileTypeBox.CompatibleBrands.Add("mp42");
            fileTypeBox.CompatibleBrands.Add("msdh");
            fileTypeBox.CompatibleBrands.Add("msix");
            // default‐base is‐moof flag
            fileTypeBox.CompatibleBrands.Add("iso5");
            // styp
            fileTypeBox.CompatibleBrands.Add("iso6");
            fileTypeBox.ToBuffer(ref writer);

            //moov
            var movieBox = new MovieBox
            {
                MovieHeaderBox = new MovieHeaderBox(0, 2)
                {
                    CreationTime = 0,
                    ModificationTime = 0,
                    Duration = 0,
                    Timescale = 1000,
                    NextTrackID = 99
                },
                //trak
                TrackBox = new TrackBox
                {
                    TrackHeaderBox = new TrackHeaderBox(0, 3)
                    {
                        CreationTime = 0,
                        ModificationTime = 0,
                        TrackID = VideoTrackID,
                        Duration = 0,
                        TrackIsAudio = false,
                        Width = (UInt32)avframe.Width,
                        Height = (UInt32)avframe.Height
                    },
                    MediaBox = new MediaBox
                    {
                        MediaHeaderBox = new MediaHeaderBox
                        {
                            CreationTime = 0,
                            ModificationTime = 0,
                            Timescale = 1000,
                            Duration = 0
                        },
                        HandlerBox = new HandlerBox
                        {
                            HandlerType = HandlerType.vide,
                            Name = "VideoHandler"
                        },
                        MediaInformationBox = new MediaInformationBox
                        {
                            VideoMediaHeaderBox = new VideoMediaHeaderBox(),
                            DataInformationBox = new DataInformationBox
                            {
                                DataReferenceBox = new DataReferenceBox
                                {
                                    DataEntryBoxes = new List<DataEntryBox> { new DataEntryUrlBox(1) }
                                }
                            },
                            SampleTableBox = new SampleTableBox
                            {
                                SampleDescriptionBox = new SampleDescriptionBox
                                {
                                    SampleEntries = new List<SampleEntry>()
                                }
                            }
                        }
                    }
                }
            };
            //h264 
            //profileIdc profileCompat levelIdc
            //0x64       0x00            0x1e
            //avc1.64001e
            var avc1 = new AVC1SampleEntry
            {
                AVCConfigurationBox = new AVCConfigurationBox(),
                Width = (UInt16)movieBox.TrackBox.TrackHeaderBox.Width,
                Height = (UInt16)movieBox.TrackBox.TrackHeaderBox.Height
            };
            avc1.AVCConfigurationBox.AVCLevelIndication = avframe.LevelIdc;
            avc1.AVCConfigurationBox.AVCProfileIndication = avframe.ProfileIdc;
            avc1.AVCConfigurationBox.ProfileCompatibility = avframe.ProfileCompat;
            avc1.AVCConfigurationBox.PPSs = new List<Byte[]>() { avframe.PPS.RawData };
            avc1.AVCConfigurationBox.SPSs = new List<Byte[]>() { avframe.SPS.RawData };
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SampleDescriptionBox.SampleEntries.Add(avc1);
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.TimeToSampleBox = new TimeToSampleBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SyncSampleBox = new SyncSampleBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SampleToChunkBox = new SampleToChunkBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.SampleSizeBox = new SampleSizeBox();
            movieBox.TrackBox.MediaBox.MediaInformationBox.SampleTableBox.ChunkOffsetBox = new ChunkOffsetBox();
            movieBox.MovieExtendsBox = new MovieExtendsBox
            {
                TrackExtendsBoxs = new List<TrackExtendsBox>()
            };
            //trex
            var trex = new TrackExtendsBox
            {
                TrackID = VideoTrackID,
                DefaultSampleDescriptionIndex = SampleDescriptionIndex,
                DefaultSampleDuration = 0,
                DefaultSampleSize = 0,
                DefaultSampleFlags = 0
            };
            movieBox.MovieExtendsBox.TrackExtendsBoxs.Add(trex);
            movieBox.ToBuffer(ref writer);

            var data = writer.FlushAndGetArray();
            return data;
        }
        finally
        {
            Fmp4ArrayPool.Return(buffer);
        }
    }

    /// <summary>
    /// 编码其他视频数据盒子
    /// styp sidx moof mdat
    /// ...
    /// styp sidx moof mdat
    /// </summary>
    /// <returns></returns>
    public Byte[] OtherVideoBox(in List<H264NALU> nalus, in String key, in Boolean keyframe)
    {
        var buffer = Fmp4ArrayPool.Rent(CacheSize);
        var writer = new Fmp4MessagePackWriter(buffer);
        try
        {
            //styp
            var stypTypeBox = new SegmentTypeBox
            {
                MajorBrand = "msdh",
                MinorVersion = "\0\0\0\0"
            };
            stypTypeBox.CompatibleBrands.Add("isom");
            stypTypeBox.CompatibleBrands.Add("mp42");
            stypTypeBox.CompatibleBrands.Add("msdh");
            stypTypeBox.CompatibleBrands.Add("msix");
            stypTypeBox.CompatibleBrands.Add("iso5");
            stypTypeBox.CompatibleBrands.Add("iso6");
            stypTypeBox.ToBuffer(ref writer);

            //mdat
            var mediaDataBox = new MediaDataBox
            {
                Data = new List<Byte[]>()
            };
            UInt32 sampleSize = 0;
            var truns = new List<TrackRunBox.TrackRunInfo>();
            foreach (var n in nalus)
            {
                sampleSize += (UInt32)(n.RawData.Length + n.StartCodePrefix.Length);
                mediaDataBox.Data.Add(n.RawData);
            }
            truns.Add(new TrackRunBox.TrackRunInfo()
            {
                SampleDuration = DefaultSampleDuration,
                SampleSize = sampleSize,
            });
            if (!TrackInfos.TryGetValue(key, out var trackInfo))
            {
                trackInfo = new TrackInfo { SN = 1, SubsegmentDuration = 0 };
                TrackInfos.Add(key, trackInfo);
            }
            if (trackInfo.SN == 0)
            {
                trackInfo.SN = 1;
            }
            //sidx
            var segmentIndexBox = new SegmentIndexBox(1)
            {
                ReferenceID = 1,
                EarliestPresentationTime = trackInfo.SubsegmentDuration,
                SegmentIndexs = new List<SegmentIndexBox.SegmentIndex>()
        {
             new SegmentIndexBox.SegmentIndex
             {
                  SubsegmentDuration=DefaultSampleDuration
             }
        }
            };
            segmentIndexBox.ToBuffer(ref writer);

            var current1 = writer.GetCurrentPosition();
            //moof
            var movieFragmentBox = new MovieFragmentBox
            {
                MovieFragmentHeaderBox = new MovieFragmentHeaderBox
                {
                    SequenceNumber = trackInfo.SN
                },
                TrackFragmentBox = new TrackFragmentBox
                {
                    TrackFragmentHeaderBox = new TrackFragmentHeaderBox(TfhdFlags)
                    {
                        TrackID = VideoTrackID,
                        SampleDescriptionIndex = SampleDescriptionIndex,
                        DefaultSampleSize = sampleSize,
                        DefaultSampleDuration = DefaultSampleDuration,
                        DefaultSampleFlags = Fmp4Constants.TFHD_FLAG_VIDEO_TPYE
                    },
                    TrackFragmentBaseMediaDecodeTimeBox = new TrackFragmentBaseMediaDecodeTimeBox
                    {
                        BaseMediaDecodeTime = trackInfo.SubsegmentDuration
                    }
                }
            };
            //update trackInfo
            trackInfo.SubsegmentDuration += DefaultSampleDuration;
            trackInfo.SN++;
            TrackInfos[key] = trackInfo;
            //trun
            if (keyframe)
            {
                movieFragmentBox.TrackFragmentBox.TrackRunBox = new TrackRunBox(1, flags:
                                                        Fmp4Constants.TRUN_FLAG_DATA_OFFSET_PRESENT |
                                                        Fmp4Constants.TRUN_FLAG_FIRST_SAMPLE_FLAGS_PRESENT);
            }
            else
            {
                movieFragmentBox.TrackFragmentBox.TrackRunBox = new TrackRunBox(1, Fmp4Constants.TRUN_FLAG_DATA_OFFSET_PRESENT);
            }
            movieFragmentBox.TrackFragmentBox.TrackRunBox.FirstSampleFlags = Fmp4Constants.TREX_FLAG_SAMPLE_DEPENDS_ON_I_PICTURE;
            movieFragmentBox.TrackFragmentBox.TrackRunBox.TrackRunInfos = truns;
            movieFragmentBox.ToBuffer(ref writer);
            //mdat
            mediaDataBox.ToBuffer(ref writer);
            var current2 = writer.GetCurrentPosition();
            foreach (var postion in segmentIndexBox.ReferencedSizePositions)
            {
                writer.WriteUInt32Return((UInt32)(current2 - current1), postion);
            }
            var data = writer.FlushAndGetArray();
            return data;
        }
        finally
        {
            Fmp4ArrayPool.Return(buffer);
        }
    }

    private struct TrackInfo
    {
        public UInt32 SN { get; set; }
        public UInt64 SubsegmentDuration { get; set; }
    }
}
