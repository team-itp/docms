import * as React from 'react';
import 'isomorphic-fetch';

export interface Link {
    href: string;
}

export interface Tag {
    name: string;
    _links: TagLinks;
}

export interface TagLinks {
    self: Link;
}

export interface Comment {

}

export interface Document {
    modified: Date;
    comments: Comment[];
    tags: Tag[];
}

export interface DocumentDetailProps {
    documentId: number
}

interface DocumentDetailState {
    document: any
    loading: boolean;
}

export class DocumentDetail extends React.Component<DocumentDetailProps, DocumentDetailState> {
    constructor(props: DocumentDetailProps) {
        super(props);
        this.state = {
            document: null,
            loading: true
        };

        fetch('api/Documents/' + this.props.documentId)
            .then(response => response.json() as Promise<any>)
            .then(data => {
                this.setState({ document: data, loading: false });
            });
    }

    static renderDocumentContet(document: Document) {
        return <div className="document">
            <footer className="document-meta">
                <time className="document-modified">{document.modified}</time>
                <span className="document-comment">
                    {document.comments.length}
                </span>
            </footer>
            <div className="document-content">
            </div>
            <div className="entry-inner"></div>
            <p className="document-tags">
                <span className="document-tags-label">タグ:</span>
                {document.tags.forEach(t => {
                    return <a key={t.name} href={t._links.self.href} rel="tag">{t.name}</a>
                })}
            </p>
        </div>;
    }

    public render() {

        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : DocumentDetail.renderDocumentContet(this.state.document);

        return <div>
            {contents}

            <nav className="post-nav">
                <ul>
                    <li className="prev">
                        <a href="/documents/0"
                            rel="prev">
                            <div className="post-thumbnail"></div>
                            <span className="chevron-left" aria-hidden="true"></span>
                            <p className="nav-title">前の投稿</p>
                            <p className="post-title">書類49</p>
                        </a>
                    </li>
                    <li className="next">
                        <a href="/documents/3"
                            rel="next">
                            <div className="post-thumbnail"></div>
                            <span className="chevron-right" aria-hidden="true"></span>
                            <p className="nav-title">次の投稿</p>
                            <p className="post-title">書類46</p>
                        </a>
                    </li>
                </ul>
            </nav>
        </div>;
    }
}
