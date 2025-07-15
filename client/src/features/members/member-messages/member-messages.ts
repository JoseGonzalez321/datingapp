import { Component, effect, ElementRef, inject, OnInit, signal, ViewChild, viewChild } from '@angular/core';
import { MessageService } from '../../../core/services/message-service';
import { MemberService } from '../../../core/services/member-service';
import { Message } from '../../../types/message';
import { DatePipe } from '@angular/common';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  imports: [DatePipe, TimeAgoPipe, FormsModule],
  templateUrl: './member-messages.html',
  styleUrl: './member-messages.css'
})
export class MemberMessages implements OnInit {
  @ViewChild('messageEndRef') messageEndRef!: ElementRef;
  private messageService = inject(MessageService);
  private memberService = inject(MemberService);
  protected messages = signal<Message[]>([]);  
  protected messageContent = '';

  constructor() {
    effect(() => {
      // This effect will run whenever the messages signal changes
      const currentMessages = this.messages();
      if (currentMessages.length > 0) {
        this.scrollToBottom();
      }
    });
  }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    // using the member service to get the current member's ID
    const memberId = this.memberService.member()?.id;

    if (memberId) {
      this.messageService.getMessageThread(memberId)
        .subscribe({
          next: messages => this.messages.set(messages.map(message => ({
            ...message,
            currentUserSender: message.senderId != memberId
          }))),
          complete: () => this.scrollToBottom(),
        });
    }
  }

  sendMessage() {
    const recipientId = this.memberService.member()?.id;

    if (!recipientId ) return;

    this.messageService.sendMessage(recipientId, this.messageContent)
      .subscribe({
        next: message => {
          this.messages.update(messages => [...messages, {
            ...message,
            currentUserSender: true
          }]);
          this.messageContent = '';
        },
        error: err => {
          console.error(err);
        }
      });
  }

  scrollToBottom() {
    setTimeout(() => {
      if (this.messageEndRef) {
        this.messageEndRef.nativeElement.scrollIntoView({ behavior: 'smooth' });
      }
    });
  }
}
