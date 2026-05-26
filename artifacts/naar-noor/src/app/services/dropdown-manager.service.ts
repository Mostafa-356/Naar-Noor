import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DropdownManagerService {
  private closeAllSubject = new Subject<string>();
  
  closeAll$ = this.closeAllSubject.asObservable();

  closeAllExcept(exceptId: string) {
    this.closeAllSubject.next(exceptId);
  }
}
