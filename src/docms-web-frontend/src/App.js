import React from 'react';
import { createMuiTheme } from '@material-ui/core/styles';
import { blueGrey, orange } from '@material-ui/core/colors';
import { ThemeProvider } from '@material-ui/styles';
import { AppBar, Toolbar, Typography } from '@material-ui/core';

const theme = createMuiTheme({
  palette: {
    primary: {
      main: blueGrey['800'],
    },
    secondary: {
      main: orange['700'],
    },
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <div className="App">
        <AppBar position="static">
          <Toolbar>
            <Typography variant="h6">
              文書管理システム
            </Typography>
          </Toolbar>
        </AppBar>
      </div>
    </ThemeProvider>
  );
}

export default App;
