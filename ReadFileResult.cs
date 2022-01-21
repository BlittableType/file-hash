namespace BlittableType
{
    internal struct ReadFileResult
    {
        public int BytesRead { get; }
        public long Reads  { get; }

        public ReadFileResult(int bytesRead, long reads)
        {
            BytesRead = bytesRead;
            Reads = reads;
        }
    }
}