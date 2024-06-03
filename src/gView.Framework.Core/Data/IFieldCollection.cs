using gView.Framework.Core.Common;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IFieldCollection : /*IEnumerable<IField>,*/ IClone
    {
        IField FindField(string aliasname);
        IField PrimaryDisplayField { get; set; }
        IField this[int i] { get; }
        int Count { get; }
        IEnumerable<IField> ToEnumerable();
    }
}