# Digital Stokvel Web Dashboard

This is the Chairperson Dashboard for Digital Stokvel Banking, built with React + TypeScript and Vite.

## Features

- **Member Management**: View and manage stokvel group members
- **Contribution Tracking**: Monitor group contributions and payment status
- **Payout Approval**: Approve and manage payouts (dual approval with Treasurer)
- **Reports & Export**: Generate financial reports and export data

## Technology Stack

- **React 19** - UI library
- **TypeScript 6** - Type-safe JavaScript
- **Vite 8** - Fast build tool and dev server
- **React Router 7** - Client-side routing

## Prerequisites

- Node.js 18+ and npm
- Access to Digital Stokvel API Gateway (default: `http://localhost:5000`)

## Getting Started

### Installation

```bash
npm install
```

### Development

Start the development server with hot reload:

```bash
npm run dev
```

The dashboard will be available at `http://localhost:3000`.

### Build

Build for production:

```bash
npm run build
```

The production build will be output to the `dist/` directory.

### Preview Production Build

Preview the production build locally:

```bash
npm run preview
```

## Project Structure

```
src/
├── main.tsx          # Application entry point
├── App.tsx           # Main App component with routing
├── App.css           # App-specific styles
├── index.css         # Global styles
└── vite-env.d.ts     # Vite type definitions
```

## API Integration

The dashboard communicates with the Digital Stokvel API Gateway. API proxy is configured in `vite.config.ts`:

- Local development: `http://localhost:5000/api`
- Production: Configure via environment variables

## Design System

The dashboard follows a clean, modern design with:
- Responsive grid layout
- Dark/light mode support
- Card-based UI components
- Gradient accents (green to blue)

## Features Roadmap

- [x] **Task 3.4.1**: Set up React + TypeScript project ✅
- [ ] **Task 3.4.2**: Implement authentication and authorization
- [ ] **Task 3.4.3**: Implement member management dashboard
- [ ] **Task 3.4.4**: Implement contribution tracking interface
- [ ] **Task 3.4.5**: Implement payout approval interface
- [ ] **Task 3.4.6**: Implement reporting and export functionality
- [ ] **Task 3.4.7**: Add responsive design for tablet support

## Configuration

### Environment Variables

Create a `.env` file for environment-specific configuration:

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_APP_TITLE=Digital Stokvel Dashboard
```

## Contributing

This project is part of the Digital Stokvel Banking MVP implementation.

## License

MIT License - See LICENSE file for details.
