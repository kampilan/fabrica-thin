
// ReSharper disable UnusedMember.Global

/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Fabrica.Persistence.Entities;
using Fabrica.Utilities.Types;

// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace Fabrica.Persistence.Audit;

/// <summary>
/// Audit Journal Entity. Used by the auditing system to persist audit journals to
/// the database
/// </summary>
public class AuditJournal: BaseEntity<AuditJournal>, IEntity
{

    public long Id { get; protected set; } = default;

    public string Uid { get; set; } = Ulid.NewUlid();

    public virtual string UnitOfWorkUid { get; set; } = string.Empty;

    public virtual string SubjectUid { get; set; } = string.Empty;

    public virtual string SubjectDescription { get; set; } = string.Empty;

    public virtual DateTime Occurred { get; set; }

    public virtual string TypeCode { get; set; } = string.Empty;

    public virtual string Entity { get; set; } = string.Empty;

    public virtual string EntityUid { get; set; } = string.Empty;

    public virtual string EntityDescription { get; set; } = string.Empty;


    public virtual string PropertyName { get; set; } = string.Empty;

    public virtual string PreviousValue { get; set; } = string.Empty;

    public virtual string CurrentValue { get; set; } = string.Empty;

    public override string GetUid()
    {
        return Uid;
    }

}