﻿//
// Copyright Fela Ameghino 2015-2025
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
namespace Telegram.Td.Api
{
    public partial class UpdateUserIsContact
    {
        public UpdateUserIsContact(long userId)
        {
            UserId = userId;
        }

        public long UserId { get; set; }
    }
}
