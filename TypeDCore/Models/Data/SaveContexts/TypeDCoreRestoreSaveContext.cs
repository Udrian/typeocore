using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Data.SaveContexts
{
    public class TypeDCoreRestoreSaveContext : SaveContext
    {
        // Properties
        public Dictionary<string, string> RestoreCodes { get; set; }

        // Constructors
        public override void Init(IResourceModel resourceModel, object param = null)
        {
            RestoreCodes = new Dictionary<string, string>();
        }

        // Functions
        public override Task SaveAction(Project project)
        {
            return Task.Run(() =>
            {
                foreach (var restoreCode in RestoreCodes)
                {
                    File.WriteAllText(restoreCode.Key, restoreCode.Value);
                }
            });
        }
    }
}
