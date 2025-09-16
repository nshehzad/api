import { AfterViewInit, ViewChild, Component, OnInit } from '@angular/core';
import { DataService } from '../../data.service';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Roles, Resources, ResourceColumnListings, ResourceRoles, policyRoles, userRoles } from '../../roles';
import { PaginationComponent } from '@app/features/components/shared/pagination/pagination.component';
import { ResourceTypeaheadBasicComponent } from './type-ahead-resources';

@Component({
  selector: 'app-column-resources',
  templateUrl: './column-resources.component.html',
  styleUrls: ['./column-resources.component.css'],
  standalone: false
})

 

export class ColumnResourcesComponent implements OnInit {
  @ViewChild('pagination') pagination: PaginationComponent;
  searchForm: FormGroup;
  roles!: Roles | any;
  resources!: Resources | any;
  rolesResources: ResourceRoles[] = [];
  resourceColumnListings: ResourceColumnListings | any;
  public roleCode: any = "";
  public resourceName: any = "";
  public role: any = "";
  public resource: any = "";
  public userRoles: any = [];
  public selected: string;
  public isLoading: boolean = false;
  public error = "";
  public defaultCol: string = 'logId';
  public sortColumn: string = sessionStorage.getItem("orderBy") != null ? sessionStorage.getItem("orderBy").split(" ")[0] : this.defaultCol;
  public sortDirection: string = sessionStorage.getItem("orderBy") != null ? sessionStorage.getItem("orderBy").split(" ")[1] : 'desc';

  constructor(private dataService: DataService) { }
  selectedResource: string=null;
  selectedRole:string;
  selectedColumn:string;

  ngOnInit(): void {
    this.searchForm = new FormGroup({
      role: new FormControl(this.role),
      resource: new FormControl(this.resource),
    });

    this.GetRoleDropdown();
  }

 
  //Initilize values for pagaination
  get pageSize():number {
    return this.pagination?.pageSize ?? 20;
  }
  set pageSize(val:number) {
    this.pagination.pageSize = val;
   
  }

  public sort(column: string) {
    this.sortColumn =column;
    this.sortDirection = column.trim().toLowerCase() == this.sortColumn.trim().toLowerCase()  && this.sortDirection == 'asc'? 'desc': 'asc';
    this.SearchResources();
  }

  GetRoleDropdown() {
    try {
      this.dataService.getRoles()
        .subscribe({
          next: (response) => {
            this.roles = response;
            this.isLoading = false;
            //this.pagination.total = response.ItemsCount;
          },
          error: (e) => {
            this.error = e.message;
            this.roles = null;
            console.error(e)
          }
        });
    }
    catch (e) {
      this.error = e.message;
      console.error(e)
    }
  }

  onResourceSelected(resource: string) {
    this.selectedResource = resource;
  }

  onRoleSelected() {
    let role = this.searchForm.get("role").value == undefined || this.searchForm.get("role").value == null ? '' : this.searchForm.get("role").value.trim();
    this.selectedRole = role;

    }


  onColumnSelected(column: string) {
    //let column = this.searchForm.get("column").value == undefined || this.searchForm.get("column").value == null ? '' : this.searchForm.get("column").value.trim();
    this.selectedColumn = column;

    }


  SearchResources() {
    this.isLoading = true;

    let role = this.searchForm.get("role").value == undefined || this.searchForm.get("role").value == null ? '' : this.searchForm.get("role").value.trim();
    let resource = this.selectedResource == undefined || this.selectedResource == null ? '' : this.selectedResource.trim();
     let column = this.selectedColumn == undefined || this.selectedColumn == null ? '' : this.selectedColumn.trim();

    try {
      this.dataService.getFilteredResources(role, resource, column)
        .subscribe({
          next: (response) => {
            this.resourceColumnListings = response.data;
            this.isLoading = false;
            this.pagination.total = response.ItemsCount;
            console.log(this.resourceColumnListings);

            //save form to session session
            sessionStorage.setItem('role', role);
            sessionStorage.setItem('resource', resource);
            sessionStorage.setItem('column', column);
          },
          error: (e) => {
            this.error = e.message;
            this.resourceColumnListings = null;
            console.error(e)
          }
        });
    }
    catch (e) {
      this.error = e.message;
      console.error(e)
    }
  }
}



