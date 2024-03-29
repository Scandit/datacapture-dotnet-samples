﻿/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Scandit.DataCapture.ID.Data;
using USDLVerificationSample.Extensions;

namespace USDLVerificationSample.Results.Presenters
{
    public class CapturedIdResultPresenter : IResultPresenter
    {
        public IList<ResultEntry> Rows { get; protected set; }

        public CapturedIdResultPresenter(CapturedId capturedId)
        {
            this.Rows = GetCommonRows(capturedId);
        }

        protected static IList<ResultEntry> GetCommonRows(CapturedId capturedId)
        {
            if (capturedId == null)
            {
                throw new ArgumentNullException(nameof(capturedId));
            }

            return new[] {
                new ResultEntry(value: capturedId.FirstName, title: "Name"),
                new ResultEntry(value: capturedId.LastName, title: "Lastname"),
                new ResultEntry(value: capturedId.FullName, title: "Full Name"),
                new ResultEntry(value: capturedId.Sex, title: "Sex"),
                new ResultEntry(value: capturedId.DateOfBirth?.LocalDate.ToShortDateString(), title: "Date of Birth"),
                new ResultEntry(value: capturedId.Nationality, title: "Nationality"),
                new ResultEntry(value: capturedId.Address, title: "Address"),
                new ResultEntry(value: capturedId.CapturedResultTypes.GetResultTypes(), title: "Captured Result Types"),
                new ResultEntry(value: capturedId.DocumentType.GetName(), title: "Document Type"),
                new ResultEntry(value: capturedId.IssuingCountryIso, title: "Issuing Country ISO"),
                new ResultEntry(value: capturedId.IssuingCountry, title: "Issuing Country"),
                new ResultEntry(value: capturedId.DocumentNumber, title: "Document Number"),
                new ResultEntry(value: capturedId.DateOfExpiry?.LocalDate.ToShortDateString(), title: "Date of Expiry"),
                new ResultEntry(value: capturedId.DateOfIssue?.LocalDate.ToShortDateString(), title: "Date of Issue")
            };
        }
    }
}
