import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ColumnResourcesComponent } from './column-resources.component';

describe('RolesPoliciesGridComponent', () => {
  let component: ColumnResourcesComponent;
  let fixture: ComponentFixture<ColumnResourcesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ColumnResourcesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ColumnResourcesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
