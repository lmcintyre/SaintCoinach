﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SaintCoinach.Ex {
    public class MultiSheet<TMulti, TData> : IMultiSheet<TMulti, TData>
        where TMulti : IMultiRow
        where TData : IDataRow {
        #region Fields

        private readonly Dictionary<Language, ISheet<TData>> _LocalisedSheets =
            new Dictionary<Language, ISheet<TData>>();

        private Dictionary<int, TMulti> _Rows;

        #endregion

        #region Properties

        public IDictionary<int, TMulti> Rows {
            get {
                FillRows();
                return _Rows;
            }
        }

        #endregion

        #region Constructors

        #region Constructor

        public MultiSheet(ExCollection collection, Header header) {
            Collection = collection;
            Header = header;
        }

        #endregion

        #endregion

        public ExCollection Collection { get; private set; }
        public Header Header { get; private set; }
        public ISheet<TData> ActiveSheet { get { return GetLocalisedSheet(Collection.ActiveLanguage); } }
        public int Count { get { return Rows.Count; } }

        #region Get

        public ISheet<TData> GetLocalisedSheet(Language language) {
            ISheet<TData> sheet;

            if (_LocalisedSheets.TryGetValue(language, out sheet)) return sheet;

            if (!Header.AvailableLanguages.Contains(language))
                throw new NotSupportedException();

            sheet = CreateLocalisedSheet(language);
            _LocalisedSheets.Add(language, sheet);

            return sheet;
        }

        #endregion

        #region IEnumerable<TMulti> Members

        public IEnumerator<TMulti> GetEnumerator() {
            return Rows.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        #region Fill

        private void FillRows() {
            if (_Rows != null)
                return;

            ISheet<TData> sheet;
            if (_LocalisedSheets.Count > 0)
                sheet = _LocalisedSheets.Values.First();
            else if (Header.AvailableLanguages.Contains(Collection.ActiveLanguage))
                sheet = GetLocalisedSheet(Collection.ActiveLanguage);
            else
                sheet = GetLocalisedSheet(Header.AvailableLanguages.First());

            _Rows = sheet.GetAllRows().ToDictionary(_ => _.Key, _ => CreateMultiRow(_.Key));
        }

        #endregion

        #region Factory

        protected virtual TMulti CreateMultiRow(int row) {
            return (TMulti)Activator.CreateInstance(typeof(TMulti), this, row);
        }

        protected virtual ISheet<TData> CreateLocalisedSheet(Language language) {
            return new DataSheet<TData>(Collection, Header, language);
        }

        #endregion

        #region IMultiSheet<TMulti,TSource> Members

        public IEnumerable<TMulti> GetAllRows() {
            return Rows.Values;
        }

        public TMulti this[int row] { get { return Rows[row]; } }

        #endregion

        #region ISheet Members

        public string Name { get { return Header.Name; } }

        public bool ContainsRow(int row) {
            return Rows.ContainsKey(row);
        }

        IEnumerable<IRow> ISheet.GetAllRows() {
            return Rows.Values.Cast<IRow>();
        }

        IRow ISheet.this[int row] { get { return this[row]; } }

        public object this[int row, int column] { get { return this[row][column]; } }

        #endregion

        #region IMultiSheet Members

        ISheet IMultiSheet.ActiveSheet { get { return ActiveSheet; } }

        ISheet IMultiSheet.GetLocalisedSheet(Language language) {
            return GetLocalisedSheet(language);
        }

        IEnumerable<IMultiRow> IMultiSheet.GetAllRows() {
            return Rows.Values.Cast<IMultiRow>();
        }

        IMultiRow IMultiSheet.this[int row] { get { return this[row]; } }

        #endregion
    }
}
