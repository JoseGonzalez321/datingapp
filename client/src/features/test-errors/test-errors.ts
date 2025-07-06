import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  imports: [],
  templateUrl: './test-errors.html',
  styleUrl: './test-errors.css'
})
export class TestErrors {
  private http = inject(HttpClient);
  baseUrl = 'https://localhost:5001/api/';
  validationErrors = signal<string[]>([]);

  get404Error() {
    this.handleError('buggy/not-found');
  }

  get400Error() {
    this.handleError('buggy/bad-request');
  }

  get500Error() {
    this.handleError('buggy/server-error');
  }

  get401Error() {
    this.handleError('buggy/auth');
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register', {}).subscribe({
      next: response => console.log(response),
      error: error => {
        console.log(error);
        console.log("Hey made it here");
        this.validationErrors.set(error);
      }
    });
  }

  private handleError(endpoint: string) {
    this.http.get(this.baseUrl + endpoint).subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }
}
