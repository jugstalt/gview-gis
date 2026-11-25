#nullable enable

using gView.Framework.Core.Exceptions;
using gView.Framework.Core.FDB;
using gView.Interoperability.Extensions.Editing;
using System;
using System.Threading.Tasks;

namespace gView.Server.EndPoints.GeoJsonService.Extensions;

internal static class FeatureEditServiceExtensions
{
    /// <summary>
    /// Runs the edit in one edit session (one transaction), if the database supports it: all features or none.
    /// </summary>
    static public async Task ExecuteAsync(this FeatureEditService editService, Func<IFeatureEditSession?, Task<bool>> edit)
    {
        var session = await editService.BeginEditSession();

        if (session is null)
        {
            if (!await edit(null))
            {
                throw new MapServerException(editService.LastErrorMessage() ?? "edit failed");
            }
            return;
        }

        await using (session)
        {
            if (!await edit(session) || !await session.Commit())
            {
                throw new MapServerException(editService.LastErrorMessage(session) ?? "edit failed");
            }
        }
    }
}
