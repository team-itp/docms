import React from 'react';
import { Container, Typography } from '@material-ui/core';

class NotFound extends React.Component {
  render() {
    return (
      <Container maxWidth="xl">
        <Typography component="h1" variant="h2">
          404 Not Found
        </Typography>
        <Typography component="p" variant="h4">
          指定されたリソースは見つかりません。
        </Typography>
      </Container>
    )
  }
}

export default NotFound;