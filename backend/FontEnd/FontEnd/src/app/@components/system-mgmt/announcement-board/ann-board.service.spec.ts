import { TestBed } from '@angular/core/testing';

import { AnnBoardService } from './ann-board.service';

describe('AnnBoardService', () => {
  let service: AnnBoardService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AnnBoardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
