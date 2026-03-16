import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CategoryEntry } from '../../../../core/models/home-models';

@Component({
  selector: 'app-category-grid',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './category-grid.component.html',
})
export class CategoryGridComponent {
  categories = input.required<CategoryEntry[]>();
}
