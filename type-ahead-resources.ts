import { Component, EventEmitter, Input, Output, SimpleChanges } from '@angular/core';
import { NgbTypeaheadModule } from '@ng-bootstrap/ng-bootstrap';
import { Observable, OperatorFunction } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';
import { JsonPipe } from '@angular/common';
import { DataService } from '@app/core/data.service';
 

@Component({
	selector: 'ng-typeahead-resources',
	templateUrl: './type-ahead-resources.html',
	standalone:false
})
export class ResourceTypeaheadBasicComponent {
  model: any;
  @Input() selectedRole: any; // <-- Receive value from parent
  resources:any =[];
  @Output() modelChange = new EventEmitter<any>(); // Emit selected value to resource component
  constructor(private dataService: DataService) { 
 	
  }
 ngOnInit(): void {
	 this.GetResources('');
	}

  onModelChange(value: any) {
    this.modelChange.emit(value);
  }

  search: OperatorFunction<string, readonly string[]> = (text$: Observable<string>) =>
		text$.pipe(
			debounceTime(200),
			distinctUntilChanged(),
			map((term) =>
				term.length < 2 ? [] : this.resources.filter((v: string) => v.toLowerCase().indexOf(term.toLowerCase()) > -1).slice(0, 10),
			),
		); 

  ngOnChanges(changes: SimpleChanges) {
	if (changes['selectedRole']) {
		this.GetResources(this.selectedRole);
	}
	}

  GetResources(role: any) {
			try {
			this.dataService.getResourceNamesByRole(role)
				.subscribe({
				next: (response) => {
					this.resources = response.data;
				},
				error: (e) => {
 
					console.error(e)
				}
				});
			}
			catch (e) {
			//this.error = e.message;
			console.error(e)
			}
		}
}     


