import { Component, ElementRef, input, model, output, ViewChild } from '@angular/core';
import { Member, MemberParams } from '../../../types/Member';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-filter-modal',
  imports: [FormsModule],
  templateUrl: './filter-modal.html',
  styleUrl: './filter-modal.css'
})
export class FilterModal {
  @ViewChild('filterModal') filterModal!: ElementRef<HTMLDialogElement>;
  closeModal = output();
  submitData = output<MemberParams>();
  memberParams = model(new MemberParams());

  constructor() {
    const filters = localStorage.getItem('filters');
      if (filters) {
        this.memberParams.set(JSON.parse(filters));
      }
  }

  open() {
    this.filterModal.nativeElement.showModal();
  }

  close() {
    this.filterModal.nativeElement.close();
    this.closeModal.emit();
  }

  submit() {   
    this.submitData.emit(this.memberParams())
    this.close();
  }

  onMinAgeChange() {
    if (this.memberParams().minAge < 18) {
      this.memberParams().minAge = 18;
    }
  }

  onMaxAgeChange() {
    if (this.memberParams().maxAge < this.memberParams().minAge) {
      this.memberParams().maxAge = this.memberParams().minAge;
    }
  }
}