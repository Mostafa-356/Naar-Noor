import { TestBed } from '@angular/core/testing';
import { DropdownManagerService } from '../dropdown-manager.service';

describe('DropdownManagerService', () => {
  let service: DropdownManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DropdownManagerService],
    });
    service = TestBed.inject(DropdownManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should expose closeAll$ observable', () => {
    expect(service.closeAll$).toBeTruthy();
  });

  it('should emit the exceptId when closeAllExcept is called', (done) => {
    const expectedId = 'dropdown-nav';

    service.closeAll$.subscribe((id) => {
      expect(id).toBe(expectedId);
      done();
    });

    service.closeAllExcept(expectedId);
  });

  it('should emit a different id on subsequent calls', () => {
    const emitted: string[] = [];

    service.closeAll$.subscribe((id) => emitted.push(id));

    service.closeAllExcept('first-dropdown');
    service.closeAllExcept('second-dropdown');

    expect(emitted).toEqual(['first-dropdown', 'second-dropdown']);
  });

  it('should emit empty string if passed empty string', (done) => {
    service.closeAll$.subscribe((id) => {
      expect(id).toBe('');
      done();
    });

    service.closeAllExcept('');
  });

  it('multiple subscribers should all receive the emission', () => {
    const received1: string[] = [];
    const received2: string[] = [];

    service.closeAll$.subscribe((id) => received1.push(id));
    service.closeAll$.subscribe((id) => received2.push(id));

    service.closeAllExcept('my-dropdown');

    expect(received1).toEqual(['my-dropdown']);
    expect(received2).toEqual(['my-dropdown']);
  });
});
