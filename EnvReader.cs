using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.SystemFunctions;

class EnvReader
{
    static EnvReader()
    {

    }

    internal static IEnumerable<string> GetWriteRegions()
    {
        // TODO: Cache those to avoid rereading for every call
        return ParseEnvironmentVariable("WRITE_REGIONS");
    }

    private static IEnumerable<string> ParseEnvironmentVariable(string envVar)
    {
        IEnumerable<string> response = null;
        var locations = Environment.GetEnvironmentVariable(envVar);

        if (!string.IsNullOrEmpty(locations))
        {
            response = locations.Split(new char[] { ':' });
        }
        return response;
    }

    internal static IEnumerable<string> GetReadRegions()
    {
        // TODO: Cache those to avoid rereading for every call
        return ParseEnvironmentVariable("READ_REGIONS");
    }

}