using System;
using System.IO; 
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core;

public class CompressionUtils : ILoggable
{

	private static readonly CompressionUtils _instance = new CompressionUtils(); // only for logging

	// uncompress gzip byte array
	public static byte[] uncompress(byte[] compressedBytes)
	{

        // Semi-arbitrary choice of 16 KB for the stream window to process.  The goal here is to pick
        // a number that's large enough to reduce the number of processing iterations, but small enough
        // to avoid being a huge memory / garbage collection hit.
        const uint STREAM_WINDOW_SIZE = 16 * 1024;

        byte[] uncompressBytes = null;

		try {

            uint uncompressedSize = 0;

            // Pull the uncompressed size out of the raw GZip bytes so that we can size our output stream appropriately.
            // Without this, on larger streams the MemoryStream will get resized multiple times which thrashes the garbage collector.
            if (compressedBytes.Length > 4)
            {
                // In a gzip file, the last four bytes are the uncompressed size, assuming the 
                // total uncompressed size is < 4 GB, which is a safe assumption for our data.
                // https://tools.ietf.org/html/rfc1952#page-5
                uncompressedSize =
                    (((uint)compressedBytes[compressedBytes.Length - 4]) << 0) |
                    (((uint)compressedBytes[compressedBytes.Length - 3]) << 8) |
                    (((uint)compressedBytes[compressedBytes.Length - 2]) << 16) |
                    (((uint)compressedBytes[compressedBytes.Length - 1]) << 24);
            }
            else
            {
                _instance.LogError("Compressed data too small to have length field: " + compressedBytes.Length);
            }

            uncompressBytes = new byte[uncompressedSize];

            // Uncompress the GZipped memory stream.
            using (MemoryStream stream = new MemoryStream(compressedBytes))
				using (GZipInputStream zip = new GZipInputStream(stream))
			{  
				MemoryStream outputStream = new MemoryStream(uncompressBytes);
				StreamUtils.Copy(zip, outputStream, new byte[STREAM_WINDOW_SIZE]);
			}
		} catch (Exception e) {
			_instance.LogError( "Unable to uncompress bytes: " + e.ToString () );
		}

		return uncompressBytes;
	}

}

