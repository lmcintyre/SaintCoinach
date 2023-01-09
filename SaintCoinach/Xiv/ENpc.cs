using System;
using System.Collections.Generic;
using System.Linq;

using SaintCoinach.Xiv.Collections;

namespace SaintCoinach.Xiv {
    public class ENpc : IQuantifiableXivString {
        #region Fields

        private ENpcBase _Base;
        private ENpcResident _Resident;

        #endregion

        #region Properties

        public int Key { get; private set; }
        public ENpcCollection Collection { get; private set; }
        public ENpcResident Resident { get { return _Resident ?? (_Resident = Collection.ResidentSheet[Key]); } }
        public ENpcBase Base { get { return _Base ?? (_Base = Collection.BaseSheet[Key]); } }
        public Text.XivString Singular { get { return Resident.Singular; } }
        public Text.XivString Plural { get { return Collection.Collection.ActiveLanguage == Ex.Language.Japanese ? Singular : Resident.Plural; } }
        public Text.XivString Title { get { return Resident.Title; } }

        #endregion

        #region Constructors

        public ENpc(ENpcCollection collection, int key) {
            Key = key;
            Collection = collection;
        }

        #endregion

        #region Build

        private Level[] BuildLevels() {
            return Collection.Collection.GetSheet<Level>().Where(_ => _.Object?.Key == Key).ToArray();
        }
        #endregion

        public override string ToString() {
            return Resident.Singular;
        }

        #region IQuantifiableName Members
        string IQuantifiable.Singular {
            get { return Singular; }
        }

        string IQuantifiable.Plural {
            get { return Plural; }
        }
        #endregion
    }
}
