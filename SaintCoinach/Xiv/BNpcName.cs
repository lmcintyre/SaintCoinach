using System;
using System.Collections.Generic;
using System.Linq;
using SaintCoinach.Ex.Relational;

namespace SaintCoinach.Xiv {
    public class BNpcName : XivRow, IQuantifiableXivString {

        #region Fields
        private ILocation[] _Locations;
        private Item[] _Items;
        private InstanceContent[] _InstanceContents;
        #endregion

        #region Properties

        public Text.XivString Singular { get { return AsString("Singular"); } }
        public Text.XivString Plural { get { return Sheet.Collection.ActiveLanguage == Ex.Language.Japanese ? Singular : AsString("Plural"); } }
        
        #endregion

        #region Constructors

        public BNpcName(IXivSheet sheet, IRelationalRow sourceRow) : base(sheet, sourceRow) { }

        #endregion

        public override string ToString() {
            return Singular;
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
