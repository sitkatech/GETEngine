using Olsson.GET.Common.DataContracts.Runs;
using System.Collections.Generic;
using Olsson.GET.Common.DataContracts.Models;

namespace Olsson.GET.Engines.RunDataParse
{
    public interface IRunDataParseEngine
    {
        RunCanalInputParseResult ParseCanalRunDataFromFile(byte[] data, Model model);

        RunWellInputParseResult ParseWellRunDataFromFile(byte[] data, Model model);

        RunWellParticleInputParseResult ParseWellParticleRunDataFromFile(byte[] data, Model model);

        byte[] CanalRunDataToCsv(List<RunCanalInput> data);

        byte[] WellRunDataToCsv(List<RunWellInput> data);

        byte[] WellParticleRunDataToCsv(List<RunWellParticleInput> data);
    }
}
