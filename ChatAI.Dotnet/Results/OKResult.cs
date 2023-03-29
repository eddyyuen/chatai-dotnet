namespace ChatAI.Dotnet.Results
{
    public class OKResult
    {
        public OKResult(string data)
        {
            this.code = 200;
            this.data = data;
        }
        public OKResult(int code, string data)
        {
            this.code = code;
            this.data = data;
        }

        public int code { get; set; }
        public string data { get; set; }
    }
}
