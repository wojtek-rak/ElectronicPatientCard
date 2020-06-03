using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicPatientCard.Services
{
    public interface IDataSourceService
    {
        T GetData<T>(string path);
    }
}
