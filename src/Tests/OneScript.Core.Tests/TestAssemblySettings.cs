/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Xunit;

// Движок рассчитан на один экземпляр в процессе: часть его состояния статическая (кэш значений
// перечислений ClrEnumWrapperCached, AttachedScriptsFactory). Тесты создают свои движки,
// и параллельные классы тестов портили бы это состояние друг другу
[assembly: CollectionBehavior(DisableTestParallelization = true)]
