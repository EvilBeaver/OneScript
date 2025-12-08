/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Exceptions;
using OneScript.Localization;

namespace OneScript.StandardLibrary.Collections.Exceptions
{
    public class ColumnException : RuntimeException
    {
        public ColumnException(BilingualString message, Exception innerException) : base(message,
            innerException)
        {
        }

        public ColumnException(BilingualString message) : base(message)
        {
        }

        public static ColumnException WrongColumnName()
        {
            return new ColumnException(new BilingualString(
                "Неверное имя колонки",
                "Wrong column name"));
        }

        public static ColumnException WrongColumnName(string columnName)
        {
            return new ColumnException(new BilingualString(
                $"Неверное имя колонки '{columnName}'",
                $"Wrong column name '{columnName}'"));
        }

        public static ColumnException DuplicatedColumnName(string columnName)
        {
            return new ColumnException(new BilingualString(
                $"Колонка '{columnName}' уже есть",
                $"Column '{columnName}' already exists"));
        }


        public static ColumnException ColumnsMixed(string columnName)
        {
            return new ColumnException(new BilingualString(
                $"Колонка '{columnName}' не может одновременно быть колонкой группировки и колонкой суммирования",
                $"Column '{columnName}' cannot be both grouping column and summation column"));
        }

    }
}