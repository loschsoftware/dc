# Compiler Architecture Modernization

- Remove static ``MessageWriter`` and global state (``Dassie.Meta``) – instead pass around ``DiagnosticManager`` or ``Compilation``
- Remove old code generation/semantic analysis backend – new architecture based on syntax trees, binding and ``DassieType`` model.
