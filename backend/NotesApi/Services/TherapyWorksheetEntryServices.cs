using NotesApi.Models;

namespace NotesApi.Services;

public class SelfValidationEntryService : WorksheetEntryServiceBase<SelfValidationEntry>
{
    public SelfValidationEntryService(MongoDbService mongoDb) : base(mongoDb, "selfValidationEntries") { }
}

public class SelfConfidenceEntryService : WorksheetEntryServiceBase<SelfConfidenceEntry>
{
    public SelfConfidenceEntryService(MongoDbService mongoDb) : base(mongoDb, "selfConfidenceEntries") { }
}

public class SelfWorthEntryService : WorksheetEntryServiceBase<SelfWorthEntry>
{
    public SelfWorthEntryService(MongoDbService mongoDb) : base(mongoDb, "selfWorthEntries") { }
}

public class SchemaAbandonmentEntryService : WorksheetEntryServiceBase<SchemaAbandonmentEntry>
{
    public SchemaAbandonmentEntryService(MongoDbService mongoDb) : base(mongoDb, "schemaAbandonmentEntries") { }
}

public class SchemaFailureEntryService : WorksheetEntryServiceBase<SchemaFailureEntry>
{
    public SchemaFailureEntryService(MongoDbService mongoDb) : base(mongoDb, "schemaFailureEntries") { }
}

public class SchemaPeoplePleasingEntryService : WorksheetEntryServiceBase<SchemaPeoplePleasingEntry>
{
    public SchemaPeoplePleasingEntryService(MongoDbService mongoDb) : base(mongoDb, "schemaPeoplePleasingEntries") { }
}

public class SchemaSuppressionEntryService : WorksheetEntryServiceBase<SchemaSuppressionEntry>
{
    public SchemaSuppressionEntryService(MongoDbService mongoDb) : base(mongoDb, "schemaSuppressionEntries") { }
}
