using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Ionic.Zip;
using Newtonsoft.Json;
using SaintCoinach.Ex;
using SaintCoinach.Ex.Relational.Definition;
using SaintCoinach.IO;
using SaintCoinach.Xiv;

using Directory = System.IO.Directory;
using File = System.IO.File;

namespace SaintCoinach {
    /// <summary>
    ///     Central class for accessing game assets.
    /// </summary>
    public class ARealmReversed {
        #region Fields

        /// <summary>
        ///     Game data collection for the data files.
        /// </summary>
        private readonly XivCollection _GameData;

        /// <summary>
        ///     Root directory of the game installation.
        /// </summary>
        private readonly DirectoryInfo _GameDirectory;

        /// <summary>
        ///     Version of the game data.
        /// </summary>
        private readonly string _GameVersion;

        /// <summary>
        ///     Pack collection for the data files.
        /// </summary>
        private readonly PackCollection _Packs;
        
        #endregion

        #region Properties

        /// <summary>
        ///     Gets the root directory of the game installation.
        /// </summary>
        /// <value>The root directory of the game installation.</value>
        public DirectoryInfo GameDirectory { get { return _GameDirectory; } }

        /// <summary>
        ///     Gets the pack collection for the data files.
        /// </summary>
        /// <value>The pack collection for the data files.</value>
        public PackCollection Packs { get { return _Packs; } }

        /// <summary>
        ///     Gets the game data collection for the data files.
        /// </summary>
        /// <value>The game data collection for the data files.</value>
        public XivCollection GameData { get { return _GameData; } }

        /// <summary>
        ///     Gets the version of the game data.
        /// </summary>
        /// <value>The version of the game data.</value>
        public string GameVersion { get { return _GameVersion; } }

        /// <summary>
        ///     Gets the version of the loaded definition.
        /// </summary>
        /// <value>The version of the loaded definition.</value>
        public string DefinitionVersion { get { return GameData.Definition.Version; } }

        /// <summary>
        ///     Gets a value indicating whether the loaded definition is the same as the game data version.
        /// </summary>
        /// <value>Whether the loaded definition is the same as the game data version.</value>
        public bool IsCurrentVersion { get { return GameVersion == DefinitionVersion; } }

        #endregion
        
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ARealmReversed" /> class.
        /// </summary>
        /// <param name="gamePath">Directory path to the game installation.</param>
        /// <param name="language">Initial language to use.</param>
        public ARealmReversed(string gamePath, Language language) : this(new DirectoryInfo(gamePath), language) { }
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="ARealmReversed" /> class.
        /// </summary>
        /// <param name="gameDirectory">Directory of the game installation.</param>
        /// <param name="language">Initial language to use.</param>
        public ARealmReversed(DirectoryInfo gameDirectory, Language language) {

            // Fix for being referenced in a .Net Core 2.1+ application (https://stackoverflow.com/questions/50449314/ibm437-is-not-a-supported-encoding-name => https://stackoverflow.com/questions/44659499/epplus-error-reading-file)
            // PM> dotnet add package System.Text.Encoding.CodePages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _GameDirectory = gameDirectory;
            _Packs = new PackCollection(Path.Combine(gameDirectory.FullName, "game", "sqpack"));
            _GameData = new XivCollection(Packs) {
                ActiveLanguage = language
            };

            _GameVersion = File.ReadAllText(Path.Combine(gameDirectory.FullName, "game", "ffxivgame.ver"));
            _GameData.Definition = ReadDefinition();
            _GameData.Definition.Compile();
        }

        #endregion

        #region Shared

        private RelationDefinition ReadDefinition() {
            var versionPath = Path.Combine("Definitions", "game.ver");
            if (!File.Exists(versionPath))
                throw new InvalidOperationException("Definitions\\game.ver must exist.");

            var version = File.ReadAllText(versionPath).Trim();
            var def = new RelationDefinition() { Version = version };
            foreach (var sheetFileName in Directory.EnumerateFiles("Definitions", "*.json")) {
                var json = File.ReadAllText(Path.Combine(sheetFileName), Encoding.UTF8);
                var obj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);
                var sheetDef = SheetDefinition.FromJson(obj);
                def.SheetDefinitions.Add(sheetDef);

                if (!_GameData.SheetExists(sheetDef.Name)) {
                    var msg = $"Defined sheet {sheetDef.Name} is missing.";
                    Debug.WriteLine(msg);
                    Console.WriteLine(msg);
                }
            }

            return def;
        }
        
        #endregion
    }
}
