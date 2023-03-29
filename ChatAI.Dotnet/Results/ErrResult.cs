namespace ChatAI.Dotnet.Results
{
    public class ErrResult
    {
        public ErrResult(string msg)
        {
            this.code = 500;
            this.msg = msg;
        }
        public ErrResult(int code, string msg)
        {
            this.code = code;
            this.msg = msg;
        }

        public int code { get; set; }
        public string msg { get; set; }
    }
}
