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
	printf("offsetof(struct tVariant, lVal) is %d\n", (int)offsetof(tVariant, lVal));
	printf("offsetof(struct tVariant, bVal) is %d\n", (int)offsetof(tVariant, bVal));
	printf("offsetof(struct tVariant, dblVal) is %d\n", (int)offsetof(tVariant, dblVal));
	printf("offsetof(struct tVariant, date) is %d\n", (int)offsetof(tVariant, date));
	printf("offsetof(struct tVariant, tmVal) is %d\n", (int)offsetof(tVariant, tmVal));
	printf("offsetof(struct tVariant, pstrVal) is %d\n", (int)offsetof(tVariant, pstrVal));
	printf("offsetof(struct tVariant, strLen) is %d\n", (int)offsetof(tVariant, strLen));
	printf("offsetof(struct tVariant, pwstrVal) is %d\n", (int)offsetof(tVariant, pwstrVal));
	printf("offsetof(struct tVariant, wstrLen) is %d\n", (int)offsetof(tVariant, wstrLen));
	printf("offsetof(struct tVariant, vt) is %d\n", (int)offsetof(tVariant, vt));
	printf("sizeof(struct tVariant) is %d\n", (int)sizeof(tVariant));
	return 0;
}
