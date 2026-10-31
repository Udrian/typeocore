using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Data.SaveContexts
{
    /// <summary>
    /// Saves queued component restore-code files for a project.
    /// </summary>
    public class TypeDCoreRestoreSaveContext : SaveContext
    {
        // Properties
        /// <summary>
        /// Gets or sets the restore code keyed by the file path to write.
        /// </summary>
        public Dictionary<string, string> RestoreCodes { get; set; }

        // Constructors
        /// <summary>
        /// Initializes the restore-code collection.
        /// </summary>
        public override void Init(IResourceModel resourceModel, object param = null)
        {
            RestoreCodes = new Dictionary<string, string>();
        }

        // Functions
        /// <summary>
        /// Writes each queued restore-code file.
        /// </summary>
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
