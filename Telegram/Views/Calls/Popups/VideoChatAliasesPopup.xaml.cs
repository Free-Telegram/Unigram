//
// Copyright Fela Ameghino 2015-2025
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using System.Collections.Generic;
using System.Linq;
using Telegram.Controls;
using Telegram.Controls.Cells;
using Telegram.Services;
using Telegram.Td.Api;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Telegram.Views.Calls.Popups
{
    public sealed partial class VideoChatAliasesPopup : ContentPopup
    {
        private readonly IClientService _clientService;
        private readonly bool _canSchedule;
        private readonly bool _channel;

        public VideoChatAliasesPopup(IClientService clientService, Chat chat, bool canSchedule, IList<MessageSender> senders)
        {
            InitializeComponent();

            _clientService = clientService;
            _canSchedule = canSchedule;
            _channel = chat.Type is ChatTypeSupergroup super && super.IsChannel;

            CloseButtonClick += Schedule_Click;

            var already = senders.FirstOrDefault(x => x.AreTheSame(chat.VideoChat.DefaultParticipantId));

            Title = chat.VideoChat.GroupCallId != 0
                ? Strings.VoipGroupDisplayAs
                : _channel
                ? Strings.StartVoipChannelTitle
                : Strings.StartVoipChatTitle;

            MessageLabel.Text = _channel
                ? Strings.VoipGroupStartAsInfo
                : Strings.VoipGroupStartAsInfoGroup;

            if (canSchedule)
            {
                CloseButtonText = _channel
                    ? Strings.VoipChannelScheduleVoiceChat
                    : Strings.VoipGroupScheduleVoiceChat;
            }

            PrimaryButtonText = _channel
                ? Strings.VoipChannelStartVoiceChat
                : Strings.VoipGroupStartVoiceChat;

            ScrollingHost.ItemsSource = senders;
            ScrollingHost.SelectedItem = already ?? senders.FirstOrDefault();

            if (clientService.TryGetSupergroup(chat, out Supergroup supergroup))
            {
                StartWith.Visibility = canSchedule && supergroup.CanManageVideoChats()
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            else if (clientService.TryGetBasicGroup(chat, out BasicGroup basicGroup))
            {
                StartWith.Visibility = canSchedule && basicGroup.CanManageVideoChats()
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            else
            {
                StartWith.Visibility = Visibility.Collapsed;
            }
        }

        public bool IsScheduleSelected { get; private set; }

        public bool IsStartWithSelected { get; private set; }

        public MessageSender SelectedSender => ScrollingHost.SelectedItem as MessageSender;

        #region Recycle

        private void OnChoosingItemContainer(ListViewBase sender, ChoosingItemContainerEventArgs args)
        {
            if (args.ItemContainer == null)
            {
                args.ItemContainer = new MultipleListViewItem(sender, false);
                args.ItemContainer.Style = ScrollingHost.ItemContainerStyle;
                args.ItemContainer.ContentTemplate = ScrollingHost.ItemTemplate;
            }

            args.IsContainerPrepared = true;
        }

        private void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.InRecycleQueue)
            {
                return;
            }
            else if (args.ItemContainer.ContentTemplateRoot is ChatShareCell content)
            {
                content.UpdateState(args.ItemContainer.IsSelected, false, true);
                content.UpdateMessageSender(_clientService, args, OnContainerContentChanging);
            }
        }

        #endregion

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScrollingHost.SelectedItem is MessageSender messageSender)
            {
                if (_clientService.TryGetUser(messageSender, out User user))
                {
                    PrimaryButtonText = string.Format(Strings.VoipGroupContinueAs, user.FullName());
                }
                else if (_clientService.TryGetChat(messageSender, out Chat chat))
                {
                    PrimaryButtonText = string.Format(Strings.VoipGroupContinueAs, _clientService.GetTitle(chat));
                }

                IsPrimaryButtonEnabled = true;
            }
            else
            {
                IsPrimaryButtonEnabled = false;
            }
        }

        private void Schedule_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IsScheduleSelected = true;
        }

        private void StartWith_Click(object sender, TextUrlClickEventArgs e)
        {
            IsStartWithSelected = true;
            Hide(ContentDialogResult.Primary);
        }
    }
}
