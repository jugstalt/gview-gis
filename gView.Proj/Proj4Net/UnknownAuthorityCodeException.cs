using System;

namespace Proj4Net
{
/**
 * Signals that an authority code is unknown 
 * and cannot be mapped to a CRS definition.
 * 
 * @author mbdavis
 *
 */
public class UnknownAuthorityCodeException : Proj4NetException 
{
	public UnknownAuthorityCodeException()
        :base()
    {
	}

	public UnknownAuthorityCodeException(String message) 
        :base(message)
    {
	}
}
}