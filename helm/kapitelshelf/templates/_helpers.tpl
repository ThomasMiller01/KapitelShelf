{{- define "kapitelshelf.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
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

{{- define "postgresql.host" -}}
{{- if .Values.global.deployPostgres -}}
{{ .Release.Name }}-postgresql.{{ .Values.global.namespace }}.svc.cluster.local:{{ .Values.postgresql.primary.service.port }}
{{- else -}}
{{ .Values.postgres.host }}
{{- end -}}
{{- end -}}

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