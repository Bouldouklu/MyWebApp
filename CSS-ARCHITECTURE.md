# CSS Architecture Guide for MyWebApp

## Overview
This document outlines the CSS organization and best practices for the MyWebApp Blazor application.

## CSS File Structure

```
wwwroot/
├── css/
│   ├── bootstrap/
│   │   └── bootstrap.min.css    # Bootstrap framework (DO NOT MODIFY)
│   └── app.css                  # Global styles and CSS variables
├── MyWebApp.styles.css          # Auto-generated bundle (DO NOT EDIT)
```

Component styles:
```
Layout/
├── MainLayout.razor.css         # Layout-specific styles
├── NavMenu.razor.css           # Navigation component styles
Pages/
├── [PageName].razor.css        # Page-specific styles
```

## CSS Loading Order (index.html)

1. **Bootstrap** - Base framework styles
2. **app.css** - Global custom styles and variables
3. **MyWebApp.styles.css** - Bundled component styles (auto-generated)

## Style Categories

### 1. CSS Variables (app.css)
All design tokens are defined as CSS custom properties in `:root`
- Colors: `--primary-color`, `--sidebar-gradient-start`, etc.
- Dimensions: `--sidebar-width`, `--top-row-height`, etc.
- Spacing: `--nav-item-padding`, `--content-padding-x`, etc.

### 2. Global Styles (app.css)
- Base resets and body styles
- Loading screen animations
- Error UI (Blazor error handling)
- Navigation icons (centralized)
- Utility classes

### 3. Component Styles (*.razor.css)
- Scoped to specific components
- Use CSS variables from app.css
- Avoid `::deep` unless absolutely necessary

## Best Practices

### DO's ✅

1. **Use CSS Variables**
   ```css
   /* Good */
   .sidebar {
     width: var(--sidebar-width);
   }
   ```

2. **Component-Specific Classes**
   ```css
   /* Good - specific naming */
   .nav-menu-item { }
   .coffee-entry-form { }
   ```

3. **Bootstrap Utilities First**
   ```html
   <!-- Good - use Bootstrap classes -->
   <div class="mb-3 text-center">
   ```

4. **Mobile-First Approach**
   ```css
   /* Good - mobile first */
   .element { /* mobile styles */ }
   @media (min-width: 641px) { /* desktop */ }
   ```

### DON'Ts ❌

1. **Avoid Generic Class Names**
   ```css
   /* Bad - too generic */
   .page { }
   .container { }
   ```

2. **Don't Override Bootstrap Directly**
   ```css
   /* Bad */
   .btn { background: red; }
   
   /* Good */
   .btn-custom { background: red; }
   ```

3. **Minimize ::deep Usage**
   ```css
   /* Avoid when possible */
   .component ::deep .child { }
   ```

4. **Don't Duplicate Styles**
   ```css
   /* Bad - same styles in multiple files */
   /* Keep shared styles in app.css */
   ```

## Adding New Styles

### For Global Features
Add to `app.css` under the appropriate section:
- New color? Add to `:root` variables
- New icon? Add to Navigation Icons section
- New utility? Add to Utility Classes section

### For New Components
1. Create `ComponentName.razor.css`
2. Use existing CSS variables
3. Keep styles scoped to that component
4. Document any special cases

### For New Pages
1. Create `PageName.razor.css` if needed
2. Reuse existing utility classes
3. Only add page-specific styles

## Troubleshooting CSS Conflicts

1. **Check Specificity**
   - Use browser DevTools
   - Look for competing selectors
   - CSS specificity calculator: 
     - Inline styles: 1000
     - IDs: 100
     - Classes: 10
     - Elements: 1

2. **Check Load Order**
   - Later files override earlier ones
   - Component styles load after global styles

3. **Check for ::deep Leaks**
   - `::deep` can affect child components unexpectedly

4. **Clear Browser Cache**
   - CSS files are often cached
   - Use hard refresh (Ctrl+Shift+R / Cmd+Shift+R)

## Color Palette Reference

```css
Primary: #1b6ec2
Primary Hover: #0a58ca
Sidebar Start: rgb(5, 39, 103)
Sidebar End: #3a0647
Star Rating: #ffc107
Code Highlight: #c02d76
```

## Responsive Breakpoints

- Mobile: <= 640.98px
- Desktop: >= 641px

## Future Improvements

- [ ] Consider CSS Modules for larger scale
- [ ] Add dark mode support with CSS variables
- [ ] Consider utility-first framework (Tailwind)
- [ ] Add CSS linting rules
- [ ] Create component library documentation