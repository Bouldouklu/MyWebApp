## 🛠️ Technology Stack# MyWebApp

A personal learning project built with Blazor WebAssembly - my journey into modern web development, GitHub workflows, and AI-assisted coding tools.

## 🎯 About This Project

This is a **hobbyist learning project** where I'm exploring and experimenting with:
- **GitHub & Version Control**: Learning Git workflows, branching, and collaboration
- **GitHub Codespaces**: Cloud-based development environments
- **Claude AI**: AI-assisted coding and problem-solving
- **Claude Code**: Command-line AI coding tools
- **Blazor WebAssembly**: Modern .NET web development
- **Full-stack development**: From UI to services and data management

This application serves as my playground for trying new technologies, patterns, and tools while building something functional and fun!

## 🚀 Features

### 📋 Todo Management
- Create, edit, and delete todo items
- Set deadlines and track overdue tasks
- Filter by status (All, Active, Completed, Overdue)
- Rich task descriptions and validation
- Statistics and progress tracking
- **⚠️ Under Development**: Currently runs locally in browser; no persistent storage yet

### ☕ Coffee Log
- Track your daily coffee consumption
- Log different coffee types and ratings
- Visual consumption patterns
- Personal coffee statistics
- **⚠️ Under Development**: Currently runs locally in browser; no persistent storage yet

### 🌤️ Weather Information
- Current weather conditions
- Weather forecasting
- Location-based weather data

### 📰 News Feeds
- **TechSpot News**: Latest technology news and reviews
- **Game Development News**: Industry updates and gaming news
- Category filtering and search functionality
- Article thumbnails and metadata

### 🏉 Rugby Calendar
- Dynamic rugby match calendar
- Multiple competitions (Six Nations, Rugby Championship, World Cup, Champions Cup)
- Live match tracking and scores
- Upcoming fixtures and recent results
- Competition filtering and team flags
- **⚠️ Under Development**: Currently shows placeholder matches; live API integration pending

## 🚧 Development Status

### Learning Focus Areas

### Pages Under Development

- **🏉 Rugby Calendar**: Match placeholders are working, but live API integration for actual past, current, and future match data is still in progress *(Learning: External API integration)*
- **📋 Todo List**: Fully functional locally but data is stored in browser memory only - no persistent storage implemented yet *(Learning: Local storage and state management)*
- **☕ Coffee Log**: Fully functional locally but data is stored in browser memory only - no persistent storage implemented yet *(Learning: Data persistence patterns)*

### Completed Features
- **🌤️ Weather Information**: Fully functional *(Learned: Service injection and HTTP clients)*
- **📰 News Feeds**: Fully functional with live data feeds *(Learned: RSS parsing and data transformation)*

- **Framework**: Blazor WebAssembly (.NET 8)
- **UI Framework**: Bootstrap 5
- **Architecture**: Component-based SPA
- **State Management**: Scoped and Singleton services
- **Styling**: CSS custom properties with responsive design
- **Build System**: MSBuild with WebAssembly publishing

## 📁 Project Structure

```
MyWebApp/
├── Pages/              # Razor pages/components
│   ├── Home.razor     # Landing page
│   ├── Todo.razor     # Todo management
│   ├── News.razor     # News feeds
│   ├── Rugby.razor    # Rugby calendar
│   └── Weather.razor  # Weather information
├── Services/          # Business logic services
│   ├── TodoService.cs
│   ├── NewsService.cs
│   ├── RugbyCalendarService.cs
│   └── WeatherService.cs
├── Models/            # Data models
├── Layout/            # Shared layout components
└── wwwroot/          # Static assets and CSS
```

## 🚧 Getting Started

### Prerequisites
- .NET 8 SDK
- Modern web browser with WebAssembly support

### Installation

1. Clone the repository
```bash
git clone [your-repo-url]
cd MyWebApp
```

2. Restore dependencies
```bash
dotnet restore
```

3. Run the application
```bash
dotnet run
```

4. Open your browser and navigate to `https://localhost:7074`

### Development Server
```bash
dotnet watch run
```

## 🎨 CSS Architecture

The application uses a structured CSS approach:
- **CSS Variables**: Centralized design tokens in `app.css`
- **Component Scoping**: Individual `.razor.css` files for components
- **Bootstrap Integration**: Utility-first approach with custom extensions
- **Responsive Design**: Mobile-first responsive layouts

See `CSS-ARCHITECTURE.md` for detailed styling guidelines.

## 🔧 Configuration

### API Integration
The application supports integration with external APIs for:
- Weather data services
- News feed providers (RSS/JSON)
- Rugby match data (SportRadar, RapidAPI)

API keys can be configured in the respective service classes.

### Service Registration
Services are registered in `Program.cs` with appropriate lifetimes:
- **Scoped**: HTTP-dependent services (Weather, News)
- **Singleton**: Local data services (Todo)

## 📱 Features by Page

| Page | Features |
|------|----------|
| Home | Dashboard overview, quick access |
| Todo | Task management, filtering, statistics |
| Weather | Current conditions, forecasts |
| News | Tech news, game dev updates, article browsing |
| Coffee | Consumption tracking, ratings |
| Rugby | Match calendar, live scores, fixtures |

## 🌐 Browser Support

- Chrome/Edge (recommended)
- Firefox
- Safari
- Mobile browsers with WebAssembly support

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

This is primarily a **personal learning project**, but I welcome:
- **Suggestions and feedback** from other developers
- **Learning resources** and best practice recommendations  
- **Bug reports** if you spot issues while exploring the code

Feel free to:
1. Open issues for discussion or suggestions
2. Share learning resources in discussions
3. Fork the repo for your own experiments

*Note: This is a learning project, so code quality and practices are evolving as I learn!*

## 📚 Learning Resources

Tools and resources that have helped me build this project:
- **Claude AI**: Code assistance, debugging help, and architecture advice
- **Claude Code**: Command-line AI coding companion
- **GitHub Codespaces**: Cloud development environment

---

Built with ❤️ and lots of learning using Blazor WebAssembly, Claude AI, and GitHub Codespaces