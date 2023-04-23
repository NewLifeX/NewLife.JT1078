using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>mfra</summary>
    public class MovieFragmentRandomAccessBox : Mp4Box, IFMp4MessagePackFormatter
    {
        /// <summary>mfra</summary>
        public MovieFragmentRandomAccessBox() : base("mfra")
        {
        }

        /// <summary>tfra</summary>
        public TrackFragmentRandomAccessBox TrackFragmentRandomAccessBox { get; set; }

        /// <summary>mfro</summary>
        public MovieFragmentRandomAccessOffsetBox MovieFragmentRandomAccessOffsetBox { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);

            TrackFragmentRandomAccessBox?.ToBuffer(ref writer);
            MovieFragmentRandomAccessOffsetBox?.ToBuffer(ref writer);

            End(ref writer);

            var mfraSizePosition = writer.GetMfraSizePosition();
            if (mfraSizePosition > 0)
            {
                writer.WriteInt32Return(writer.GetCurrentPosition() - SizePosition, mfraSizePosition);
                writer.ClearMfraSizePosition();
            }
        }
    }
}