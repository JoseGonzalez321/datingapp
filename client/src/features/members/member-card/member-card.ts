import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { PresenceService } from '../../../core/services/presence-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css'
})
export class MemberCard {
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  member = input.required<Member>();
  protected hasLiked = computed(() => {
    return this.likesService.likeIds().includes(this.member().id);
  });
  protected isOnline = computed(() => {
    return this.presenceService.onlineUsers().includes(this.member().id);
  });


  toggleLike(event: Event) {
    event.stopPropagation();
    this.likesService.toggleLike(this.member().id)
  }
}
