using System.Text;

namespace Unite.Analysis.Web.Configuration.Options;

public class ApiOptions
{
    public byte[] Key
    {
        get
        {
            var key = Environment.GetEnvironmentVariable("UNITE_API_KEY");

            if (key == null)
                throw new ArgumentNullException("'UNITE_API_KEY' environment variable has to be set");

            if (key.Length != 32)
                throw new ArgumentOutOfRangeException("'UNITE_API_KEY' environment variable has to be a 32 bit string");

            return Encoding.ASCII.GetBytes(key);
        }
    }
}
