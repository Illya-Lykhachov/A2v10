﻿
using System;
using System.Collections.Generic;

namespace A2v10.Data
{
    public class ElementMetadata
    {
        IDictionary<String, FieldMetadata> _fields = new Dictionary<String, FieldMetadata>();

        public String Id { get; private set; }
        public String Name { get; private set; }
        public String Items { get; private set; }
        public Boolean IsArrayType { get; set; }

        internal IDictionary<String, FieldMetadata> Fields { get { return _fields; } }

        public void AddField(FieldInfo field, DataType type)
		{
			if (!field.IsVisible)
				return;
			if (IsFieldExists(field.PropertyName, type))
				return;
			FieldMetadata fm = new FieldMetadata(field, type);
			_fields.Add(field.PropertyName, fm);
			switch (field.SpecType)
			{
				case SpecType.Id:
					Id = field.PropertyName;
					break;
				case SpecType.Name:
					Name = field.PropertyName;
					break;
			}
		}

		public Int32 FieldCount { get { return _fields.Count; } }

		public Boolean ContainsField(String field)
		{
			return _fields.ContainsKey(field);
		}

		bool IsFieldExists(String name, DataType dataType)
		{
			FieldMetadata fm;
			if (_fields.TryGetValue(name, out fm))
			{
				if (fm.DataType != dataType)
					throw new DataLoaderException($"Invalid property '{name}'. Type mismatch.");
				return true;
			}
			return false;
		}
	}
}
