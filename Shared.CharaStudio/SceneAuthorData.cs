using BepInEx;
using BepInEx.Logging;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Maker.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using ExtensibleSaveFormat;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using Studio;
using UniRx;

namespace KKAPI
{
    [BepInPlugin(GUID, "Scene Author Data", KoikatuAPI.VersionConst)]
    [Browsable(false)]
    internal class SceneAuthorData : BaseUnityPlugin
    {
        private const string DefaultNickname = "Anonymous";
        private const string GUID = "marco.authordata";

        private static ManualLogSource _logger;
        private static ConfigEntry<string> _nickname;
        private MakerText _authorText;

        private static string CurrentNickname
        {
            get
            {
                var n = _nickname.Value;
                return string.IsNullOrEmpty(n) ? DefaultNickname : n;
            }
        }

        private void Start()
        {
            if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio) return;

            _logger = Logger;

            string configPath = Path.Combine(UserData.Path, "config/AuthorData.cfg");
            var configExists = File.Exists(configPath);
            var cfg = new ConfigFile(configPath, false, Info.Metadata);
            _nickname = cfg.Bind("AuthorInformation", "Nickname", DefaultNickname, "Your nickname that will be saved to your cards and used in the card filenames.");

            if(!configExists)
            {
                var legacyConfig = Config.Bind("", "Nickname", "Your nickname that will be saved to your cards and used in the card filenames.", DefaultNickname);
                _nickname.Value = legacyConfig.Value;
            }

            StudioSaveLoadApi.RegisterExtraBehaviour<CardAuthorDataController>(GUID);
        }

        private static string GetAuthorsText()
        {
            var authors = string.Join(" > ", MakerAPI.GetCharacterControl().GetComponent<CardAuthorDataController>().Authors.ToArray());
            var text = "Author history: " + (authors.Length == 0 ? "[Empty]" : authors);
            return text;
        }

        //private void MakerAPI_RegisterCustomSubCategories(object sender, RegisterSubCategoriesEvent e)
        //{
        //    _authorText = e.AddControl(new MakerText(GetAuthorsText(), MakerConstants.Parameter.Character, this));
        //
        //    var tb = e.AddControl(new MakerTextbox(MakerConstants.Parameter.Character, "Your nickname", DefaultNickname, this));
        //    tb.Value = CurrentNickname;
        //    tb.ValueChanged.Subscribe(
        //        s =>
        //        {
        //            if (string.IsNullOrEmpty(s)) tb.Value = DefaultNickname;
        //            else _nickname.Value = s;
        //        });
        //
        //    e.AddControl(new MakerText("Your nickname will be saved to the card and used in the card's filename. This setting is global.", MakerConstants.Parameter.Character, this) { TextColor = MakerText.ExplanationGray });
        //}

        private void MakerApiOnReloadCustomInterface(object sender, EventArgs eventArgs)
        {
            if (_authorText != null)
            {
                var text = GetAuthorsText();
                _authorText.Text = text;
            }
        }

        private sealed class CardAuthorDataController : KKAPI.Studio.SaveLoad.SceneCustomFunctionController
        {
            //todo have list of imported scenes alongsite list of authors. each list has edit dates and import list has filenames and either last author or a copy of the full author list

            protected internal override void OnSceneLoad(SceneOperationKind operation, ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
            {
                throw new NotImplementedException();
            }

            protected internal override void OnSceneSave()
            {
                throw new NotImplementedException();
            }


            //private const string AuthorsKey = "Authors";
            //private string[] _previousAuthors;
            //
            //public IEnumerable<string> Authors => _previousAuthors ?? Enumerable.Empty<string>();
            //
            //protected override void OnCardBeingSaved(GameMode currentGameMode)
            //{
            //    var authorList = Authors.ToList();
            //
            //    if (MakerAPI.InsideMaker)
            //    {
            //        if (authorList.LastOrDefault() != CurrentNickname)
            //            authorList.Add(CurrentNickname);
            //    }
            //
            //    if (authorList.Any())
            //    {
            //        SetExtendedData(new PluginData
            //        {
            //            version = 1,
            //            data = { { AuthorsKey, MessagePack.LZ4MessagePackSerializer.Serialize(authorList.ToArray()) } }
            //        });
            //    }
            //    else
            //        SetExtendedData(null);
            //}
            //
            //protected override void OnReload(GameMode currentGameMode)
            //{
            //    var flags = MakerAPI.GetCharacterLoadFlags();
            //    // If majority of the parts were loaded then use authors of the other card, else keep current
            //    if (ReplacedParts(flags) > 3)
            //    {
            //        // Flags is null when starting maker and loading chika, our otside maker. Do NOT add the unknown author tag in these cases or cards based on chika would have it too
            //        _previousAuthors = flags == null ? null : new[] { "[Unknown]" };
            //    }
            //
            //    var data = GetExtendedData();
            //    if (data != null)
            //    {
            //        if (data.data.TryGetValue(AuthorsKey, out var arr) && arr is byte[] strArr)
            //            _previousAuthors = MessagePack.LZ4MessagePackSerializer.Deserialize<string[]>(strArr);
            //    }
            //}
        }
    }
}
