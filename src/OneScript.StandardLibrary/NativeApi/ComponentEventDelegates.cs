/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.StandardLibrary.NativeApi
{
    public delegate void OnComponentError(MessageStatusEnum status, int errCode, string source, string description);
    
    public delegate void OnComponentEvent(string source, string eventName, string data);
    
    public delegate void OnComponentStatusText(string text);
}