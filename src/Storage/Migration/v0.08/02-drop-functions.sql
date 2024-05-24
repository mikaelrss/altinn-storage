DROP FUNCTION IF EXISTS storage.readinstancefromquery(
	_appId TEXT,
	_appIds TEXT[],
	_archiveReference TEXT,
	_continue_idx BIGINT,
	_created_eq TIMESTAMPTZ,
	_created_gt TIMESTAMPTZ,
	_created_gte TIMESTAMPTZ,
	_created_lt TIMESTAMPTZ,
	_created_lte TIMESTAMPTZ,
	_dueBefore_eq TEXT,
	_dueBefore_gt TEXT,
	_dueBefore_gte TEXT,
	_dueBefore_lt TEXT,
	_dueBefore_lte TEXT,
	_excludeConfirmedBy JSONB[],
	_instanceOwner_partyId INTEGER,
	_instanceOwner_partyIds INTEGER[],
	_lastChanged_eq TIMESTAMPTZ,
	_lastChanged_gt TIMESTAMPTZ,
	_lastChanged_gte TIMESTAMPTZ,
	_lastChanged_idx TIMESTAMPTZ,
	_lastChanged_lt TIMESTAMPTZ,
	_lastChanged_lte TIMESTAMPTZ,
	_org TEXT,
	_process_currentTask TEXT,
	_process_ended_eq TEXT,
	_process_ended_gt TEXT,
	_process_ended_gte TEXT,
	_process_ended_lt TEXT,
	_process_ended_lte TEXT,
	_process_isComplete BOOLEAN,
	_size INTEGER,
	_sort_ascending BOOLEAN,
	_status_isActiveOrSoftDeleted BOOLEAN ,
	_status_isArchived BOOLEAN,
	_status_isArchivedOrSoftDeleted BOOLEAN,
	_status_isHardDeleted BOOLEAN,
	_status_isSoftDeleted BOOLEAN,
	_visibleAfter_eq TEXT,
	_visibleAfter_gt TEXT,
	_visibleAfter_gte TEXT,
	_visibleAfter_lt TEXT,
	_visibleAfter_lte TEXT
	);