import { defineCollection } from 'astro:content';
import { docsLoader } from '@astrojs/starlight/loaders';
import { docsSchema } from '@astrojs/starlight/schema';
import { changelogsLoader } from 'starlight-changelogs/loader';
import { starlightTagsExtension } from 'starlight-tags/schema';

export const collections = {
    docs: defineCollection({
        loader: docsLoader(),
        schema: docsSchema({ extend: starlightTagsExtension }),
    }),
    changelogs: defineCollection({
        loader: changelogsLoader([
            {
                provider: 'github',
                base: 'changelog',
                owner: 'RocketSurgeonsGuild',
                repo: 'Extensions',
                token: import.meta.env.GH_API_TOKEN,
            },
        ]),
    }),
};
