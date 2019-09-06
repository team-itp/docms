import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Breadcrumbs, Container, Link, Typography, Toolbar } from '@material-ui/core';

function DocumentBrowser() {
  return (
    <Container maxWidth="xl">
      <Toolbar>
        <DocumentHeader />
      </Toolbar>
    </Container>
  );
}

class DocumentHeader extends React.Component {
  render() {
    return (
      <Breadcrumbs aria-label="breadcrumb">
        <Link color="inherit" component={RouterLink} to="/">
          Home
          </Link>
        <Link color="inherit" component={RouterLink} to="/Catalog">
          Catalog
          </Link>
        <Link color="inherit" component={RouterLink} to="/Catalog/Accessories">
          Accessories
          </Link>
        <Link color="inherit" component={RouterLink} to="/Catalog/Accessories/New Collection">
          New Collection
          </Link>
        <Typography color="textPrimary">Belts</Typography>
      </Breadcrumbs>
    );
  }
}

export default DocumentBrowser;