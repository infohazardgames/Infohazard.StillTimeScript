using System;
using System.Globalization;
using Infohazard.StillTimeScript.Core.Nodes;

namespace Infohazard.StillTimeScript.Core.Utility {
    public readonly struct StsValue : IEquatable<StsValue> {
        public StsValueType ValueType { get; }

        private readonly decimal _numberValue;
        private readonly StsColor _colorValue;
        private readonly object _objectValue;
        private readonly bool _boolValue;

        public decimal NumberValue =>
            ValueType == StsValueType.Number
                ? _numberValue
                : throw new InvalidOperationException($"Trying to get number value from value of type {ValueType}");

        public StsColor ColorValue =>
            ValueType == StsValueType.Color
                ? _colorValue
                : throw new InvalidOperationException($"Trying to get color value from value of type {ValueType}");

        public string StringValue =>
            ValueType == StsValueType.String
                ? (string)_objectValue
                : throw new InvalidOperationException($"Trying to get string value from value of type {ValueType}");

        public bool BoolValue =>
            ValueType == StsValueType.Bool
                ? _boolValue
                : throw new InvalidOperationException($"Trying to get bool value from value of type {ValueType}");
        
        public INode NodeValue =>
            ValueType == StsValueType.Node
                ? (INode)_objectValue
                : throw new InvalidOperationException($"Trying to get node value from value of type {ValueType}");
        
        public Resource.Resource ResourceValue =>
            ValueType == StsValueType.Resource
                ? (Resource.Resource)_objectValue
                : throw new InvalidOperationException($"Trying to get resource value from value of type {ValueType}");

        public StsValue(decimal numberValue) {
            ValueType = StsValueType.Number;
            _colorValue = default;
            _objectValue = null;
            _boolValue = false;
            _numberValue = numberValue;
        }

        public StsValue(StsColor colorValue) {
            ValueType = StsValueType.Color;
            _numberValue = 0;
            _objectValue = null;
            _boolValue = false;
            _colorValue = colorValue;
        }

        public StsValue(string stringValue) {
            ValueType = StsValueType.String;
            _numberValue = 0;
            _colorValue = default;
            _boolValue = false;
            _objectValue = stringValue;
        }

        public StsValue(bool boolValue) {
            ValueType = StsValueType.Bool;
            _numberValue = 0;
            _colorValue = default;
            _objectValue = null;
            _boolValue = boolValue;
        }

        public StsValue(INode node) {
            ValueType = StsValueType.Node;
            _numberValue = 0;
            _colorValue = default;
            _boolValue = false;
            _objectValue = node;
        }

        public StsValue(Resource.Resource resource) {
            ValueType = StsValueType.Resource;
            _numberValue = 0;
            _colorValue = default;
            _boolValue = false;
            _objectValue = resource;
        }

        public override string ToString() {
            return ValueType switch {
                StsValueType.None => "null",
                StsValueType.Number => _numberValue.ToString(CultureInfo.CurrentCulture),
                StsValueType.Color => _colorValue.ToHexString(),
                StsValueType.String => (string)_objectValue,
                StsValueType.Bool => _boolValue.ToString(),
                StsValueType.Node => ((INode)_objectValue).FullIdentifier,
                StsValueType.Resource => ((Resource.Resource)_objectValue).Identifier,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public bool ToBool() {
            return ValueType switch {
                StsValueType.None => false,
                StsValueType.Number => _numberValue != 0,
                StsValueType.Color => _colorValue != default,
                StsValueType.String => !string.IsNullOrEmpty((string)_objectValue),
                StsValueType.Bool => _boolValue,
                StsValueType.Node or StsValueType.Resource => _objectValue != null,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        
        public static StsValue Parse(string str) {
            if (decimal.TryParse(str, out decimal numValue)) {
                return new StsValue(numValue);
            } else if (str.StartsWith("#") && StsColor.TryParseHex(str, out StsColor colorValue)) {
                return new StsValue(colorValue);
            } else if (bool.TryParse(str, out bool boolValue)) {
                return new StsValue(boolValue);
            } else {
                return new StsValue(str);
            }
        }

        public static bool TryParse(string str, StsValueType type, out StsValue value) {
            switch (type) {
                case StsValueType.Number:
                    bool isInt = decimal.TryParse(str, out decimal decValue);
                    value = new StsValue(decValue);
                    return isInt;
                case StsValueType.Color:
                    bool isColor = StsColor.TryParseHex(str, out StsColor colorValue);
                    value = new StsValue(colorValue);
                    return isColor;
                case StsValueType.Bool:
                    bool isBool = bool.TryParse(str, out bool boolValue);
                    value = new StsValue(boolValue);
                    return isBool;
                case StsValueType.String:
                    value = new StsValue(str);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static StsValue Default(StsValueType type) => type switch {
            StsValueType.Number => new StsValue(0),
            StsValueType.Color => new StsValue(default(StsColor)),
            StsValueType.String => new StsValue(string.Empty),
            StsValueType.Bool => new StsValue(false),
            StsValueType.Node => new StsValue((INode)null),
            StsValueType.Resource => new StsValue((Resource.Resource)null),
            StsValueType.None => default,
            _ => throw new ArgumentOutOfRangeException(),
        };
        
        public bool Equals(StsValue other) {
            if (ValueType != other.ValueType) return false;

            return ValueType switch {
                StsValueType.None => true,
                StsValueType.Number => _numberValue == other._numberValue,
                StsValueType.Color => _colorValue.Equals(other._colorValue),
                StsValueType.String => (string)_objectValue == (string)other._objectValue,
                StsValueType.Bool => _boolValue == other._boolValue,
                StsValueType.Node  or StsValueType.Resource => ReferenceEquals(_objectValue, other._objectValue),
                _ => throw new Exception("Unexpected type"),
            };
        }

        public override bool Equals(object obj) {
            return obj is StsValue other && Equals(other);
        }

        public override int GetHashCode() {
            HashCode hashCode = new();
            hashCode.Add((int)ValueType);
            switch (ValueType) {
                case StsValueType.None:
                    break;
                case StsValueType.Number:
                    hashCode.Add(_numberValue);
                    break;
                case StsValueType.Color:
                    hashCode.Add(_colorValue);
                    break;
                case StsValueType.String:
                    hashCode.Add((string)_objectValue);
                    break;
                case StsValueType.Bool:
                    hashCode.Add(_boolValue);
                    break;
                case StsValueType.Node or StsValueType.Resource:
                    hashCode.Add(_objectValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return hashCode.ToHashCode();
        }

    }

    public enum StsValueType {
        None,
        Number,
        Color,
        String,
        Bool,
        Node,
        Resource,
    }
}
