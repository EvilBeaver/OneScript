/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#include "include/types.h"
#include "include/ComponentBase.h"
#include "include/AddInDefBase.h"
#include "include/IMemoryManager.h"
#include <stdio.h> 
#include <stddef.h>

int main()
{
	tVariant variant;
	printf("offsetof(tVariant, lVal) is %d\n", (int)offsetof(tVariant, lVal));
	printf("offsetof(tVariant, bVal) is %d\n", (int)offsetof(tVariant, bVal));
	printf("offsetof(tVariant, dblVal) is %d\n", (int)offsetof(tVariant, dblVal));
	printf("offsetof(tVariant, date) is %d\n", (int)offsetof(tVariant, date));
	printf("offsetof(tVariant, tmVal) is %d\n", (int)offsetof(tVariant, tmVal));
	printf("offsetof(tVariant, pstrVal) is %d\n", (int)offsetof(tVariant, pstrVal));
	printf("offsetof(tVariant, strLen) is %d\n", (int)offsetof(tVariant, strLen));
	printf("offsetof(tVariant, pwstrVal) is %d\n", (int)offsetof(tVariant, pwstrVal));
	printf("offsetof(tVariant, wstrLen) is %d\n", (int)offsetof(tVariant, wstrLen));
	printf("offsetof(tVariant, vt) is %d\n", (int)offsetof(tVariant, vt));
	printf("sizeof(tVariant) is %d\n\n", (int)sizeof(tVariant));
	printf("sizeof(variant.lVal) is %d\n", (int)sizeof(variant.lVal));
	printf("sizeof(variant.bVal) is %d\n", (int)sizeof(variant.bVal));
	printf("sizeof(variant.dblVal) is %d\n", (int)sizeof(variant.dblVal));
	printf("sizeof(variant.date) is %d\n", (int)sizeof(variant.date));
	printf("sizeof(variant.tmVal) is %d\n", (int)sizeof(variant.tmVal));
	printf("sizeof(variant.pstrVal) is %d\n", (int)sizeof(variant.pstrVal));
	printf("sizeof(variant.strLen) is %d\n", (int)sizeof(variant.strLen));
	printf("sizeof(variant.pwstrVal) is %d\n", (int)sizeof(variant.pwstrVal));
	printf("sizeof(variant.wstrLen) is %d\n", (int)sizeof(variant.wstrLen));
	printf("sizeof(variant.vt) is %d\n", (int)sizeof(variant.vt));
	return 0;
}
