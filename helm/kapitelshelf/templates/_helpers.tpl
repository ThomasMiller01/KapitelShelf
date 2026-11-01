{{/* --- k8s Helpers --- */}}
{{- define "kapitelshelf.name" -}}
{{- default .Chart.Name .Values.nameOverride | lower | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "kapitelshelf.labels" -}}
app.kubernetes.io/name: {{ include "kapitelshelf.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{- define "kapitelshelf.annotations" -}}
{{- if . -}}
{{- toYaml . -}}
{{- end -}}
{{- end -}}

{{/* --- API Helpers --- */}}
{{- define "kapitelshelf.api.host" -}}
{{- if .Values.api.ingress.enabled -}}
    {{- if .Values.api.ingress.tls -}}
https://{{ .Values.api.ingress.host }}{{ .Values.api.ingress.path }}
    {{- else -}}
http://{{ .Values.api.ingress.host }}{{ .Values.api.ingress.path }}
    {{- end -}}
{{- else -}}
http://localhost:{{ .Values.api.service.port }}
{{- end -}}
{{- end -}}


{{/* --- Database Helpers --- */}}
{{- define "kapitelshelf.database.host" -}}
{{- if and .Values.global.deployDatabase .Values.global.deployPostgresql -}} {{/* [DEPRECATED, remove check when removing postgres chart] */}}
{{- fail "global.deployDatabase and global.deployPostgresql cannot both be true" -}}
{{- end -}}
{{- if .Values.global.deployDatabase -}}
{{ include "kapitelshelf.name" . }}-database
{{- else if .Values.global.deployPostgresql -}} {{/* [DEPRECATED] */}}
{{ .Release.Name }}-postgresql.{{ .Values.global.namespace }}.svc.cluster.local
{{- else -}}
{{ .Values.database.service.host }}
{{- end -}}
{{- end -}}

{{/* [DEPRECATED] because of deployPostgresql, when removed just use `.Values.database.service.port` */}}
{{- define "kapitelshelf.database.port" -}}
{{- if .Values.global.deployPostgresql -}}
{{ .Values.postgresql.primary.service.port }}
{{- else -}}
{{ .Values.database.service.port }}
{{- end -}}
{{- end -}}

{{/* [DEPRECATED] because of deployPostgresql, when removed just use `.Values.database.auth.username` */}}
{{- define "kapitelshelf.database.username" -}}
{{- if .Values.global.deployPostgresql -}}
{{ .Values.postgresql.auth.username }}
{{- else -}}
{{ .Values.database.auth.username }}
{{- end -}}
{{- end -}}

{{/* [DEPRECATED] because of deployPostgresql, when removed just use `.Values.database.auth.password` */}}
{{- define "kapitelshelf.database.password" -}}
{{- if .Values.global.deployPostgresql -}}
{{ .Values.postgresql.auth.password }}
{{- else -}}
{{ .Values.database.auth.password }}
{{- end -}}
{{- end -}}
