import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import { Breadcrumbs, Container, Link, Typography, Toolbar } from '@material-ui/core';

function DocumentBrowser() {
  return (
    <Container maxWidth="xl">
      <DocumentHeader parentPath="" name="a" />
      <DocumentHeader parentPath="/" name="a" />
      <DocumentHeader parentPath="a/b" name="a" />
      <DocumentHeader parentPath="/a/b" name="" />
    </Container>
  );
}

function DocumentHeader(props) {
  let path = props.parentPath;
  let linkPath = '/docs'
  let links = path.split('/').filter(e => e).map((e, i) => {
    linkPath = linkPath + '/' + e;
    return {
      key: i,
      path: linkPath,
      name: e
    }
  });
  console.log(links);
  return (
    <Toolbar>
      <Breadcrumbs aria-label="breadcrumb" >
        <Link color="inherit" component={RouterLink} to="/">Home</Link>
        {links.map(e => (<Link key={e.key} color="inherit" component={RouterLink} to={e.path}>{e.name}</Link>))}
        {props.name && <Typography color="textPrimary">{props.name}</Typography>}
      </Breadcrumbs>
    </Toolbar>
  );
}

export default DocumentBrowser;